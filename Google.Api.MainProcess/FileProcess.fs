module FileProcess
let private timestamp = System.DateTime.UtcNow

open Google.Apis.Drive.v3
open Google.Apis.Drive.v3.Data
open System.IO
open Google.Apis.Download
open System

let rec EnsureDriveFolder (service: DriveService) (rootId: string) (baseLocal: string) (relPath: string) =
    let parts = relPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) |> Array.toList
    let mutable parentId = rootId
    for part in parts do
        if not (String.IsNullOrWhiteSpace(part)) then
            let listReq = service.Files.List()
            listReq.Q <- $"mimeType = 'application/vnd.google-apps.folder' and name = '{part}' and '{parentId}' in parents and trashed = false"
            listReq.Fields <- "files(id)"
            let found = listReq.Execute()
            parentId <-
                if found.Files.Count > 0 then found.Files.[0].Id
                else
                    let meta = File(Name = part, MimeType = "application/vnd.google-apps.folder", Parents = [| parentId |])
                    let createReq = service.Files.Create(meta)
                    createReq.Fields <- "id"
                    let folder = createReq.Execute()
                    printfn "Pasta criada: %s" part
                    folder.Id
    parentId

let GetOrCreateRootFolder (service: DriveService) =
    let folderName = "obsidian-sync"
    let listReq = service.Files.List()
    listReq.Q <- $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false"
    listReq.Fields <- "files(id)"
    let result = listReq.Execute()
    if result.Files.Count > 0 then result.Files.[0].Id
    else
        let meta = File(Name = folderName, MimeType = "application/vnd.google-apps.folder")
        let createReq = service.Files.Create(meta)
        createReq.Fields <- "id"
        let folder = createReq.Execute()
        printfn "Pasta raiz criada com ID: %s" folder.Id
        folder.Id

let UploadFile (service: DriveService, filePath: string, localRoot: string, driveRoot: string) =
    let relPath = Path.GetRelativePath(localRoot, filePath)
    let relDir = Path.GetDirectoryName(relPath)
    let parentId = if String.IsNullOrWhiteSpace(relDir) then driveRoot else EnsureDriveFolder service driveRoot localRoot relDir

    let meta = File(Name = Path.GetFileName(filePath), Parents = [| parentId |])
    use stream = new FileStream(filePath, FileMode.Open)
    let req = service.Files.Create(meta, stream, "application/octet-stream")
    req.Fields <- "id"
    let status = req.Upload()
    if status.Status <> Google.Apis.Upload.UploadStatus.Completed then failwithf "Erro upload: %A" status.Exception
    Some req.ResponseBody.Id

let FileExists (service: DriveService, fileName: string, parentId: string) =
    let listReq = service.Files.List()
    listReq.Q <- $"name = '{fileName}' and '{parentId}' in parents and trashed = false"
    listReq.Fields <- "files(id)"
    let result = listReq.Execute()
    if result.Files.Count > 0 then Some result.Files.[0].Id else None

let UpdateFile (service: DriveService, filePath: string, localRoot: string, driveRoot: string) =
    let relPath = Path.GetRelativePath(localRoot, filePath)
    let relDir = Path.GetDirectoryName(relPath)
    let parentId = if String.IsNullOrWhiteSpace(relDir) then driveRoot else EnsureDriveFolder service driveRoot localRoot relDir
    let fileName = Path.GetFileName(filePath)
    match FileExists(service, fileName, parentId) with
    | Some id ->
        use stream = new FileStream(filePath, FileMode.Open)
        let req = service.Files.Update(null, id, stream, "application/octet-stream")
        req.Fields <- "id"
        let status = req.Upload()
        if status.Status <> Google.Apis.Upload.UploadStatus.Completed then failwithf "Erro update: %A" status.Exception
        Some req.ResponseBody.Id
    | None -> None

let DeleteFile (service: DriveService, filePath: string, localRoot: string, driveRoot: string) =
    let relPath = Path.GetRelativePath(localRoot, filePath)
    let relDir = Path.GetDirectoryName(relPath)
    let parentId = if String.IsNullOrWhiteSpace(relDir) then driveRoot else EnsureDriveFolder service driveRoot localRoot relDir
    let fileName = Path.GetFileName(filePath)
    match FileExists(service, fileName, parentId) with
    | Some id ->
        let req = service.Files.Delete(id)
        req.Execute() |> ignore
        Some id
    | None -> None

let RenameFile (service: DriveService, oldPath: string, newPath: string, localRoot: string, driveRoot: string) =
    let relOld = Path.GetRelativePath(localRoot, oldPath)
    let relDir = Path.GetDirectoryName(relOld)
    let parentId = if String.IsNullOrWhiteSpace(relDir) then driveRoot else EnsureDriveFolder service driveRoot localRoot relDir
    let oldName = Path.GetFileName(oldPath)
    let newName = Path.GetFileName(newPath)
    match FileExists(service, oldName, parentId) with
    | Some id ->
        let meta = File(Name = newName)
        let req = service.Files.Update(meta, id)
        req.Fields <- "id"
        let file = req.Execute()
        Some file.Id
    | None -> None

let DownloadAllFiles (service: DriveService, driveRoot: string, localRoot: string) =
    async {
        let rec downloadFromFolder folderId localPath = async {

            let req = service.Files.List()
            req.Q <- $"'{folderId}' in parents and trashed = false"
            req.Fields <- "files(id, name, mimeType)"
            let! result = req.ExecuteAsync() |> Async.AwaitTask

            for f in result.Files do
                if not(File.Exists(Path.Combine(localRoot, f.Name))) then
                    let localTarget = Path.Combine(localPath, f.Name)
                    if f.MimeType = "application/vnd.google-apps.folder" then
                        Directory.CreateDirectory(localTarget) |> ignore
                        do! downloadFromFolder f.Id localTarget
                    else
                     printfn $"[{timestamp}] BAIXANDO {f.Name}" 
                     use stream = new FileStream(localTarget, FileMode.Create, FileAccess.Write)
                     let getReq = service.Files.Get(f.Id)
                     let! _ = getReq.DownloadAsync(stream) |> Async.AwaitTask
                     printfn $"[{timestamp}] SALVO {f.Name}" 
        }
        do! downloadFromFolder driveRoot localRoot
    }
