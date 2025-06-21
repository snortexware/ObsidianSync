module FileProcess
open TaskProcess
open TaskCreation
open Google.Apis.Drive.v3
open Google.Apis.Drive.v3.Data
open System.IO
open System.Collections
open System
open System.Threading.Tasks

let mutable isUploaded = false

let GetOrCreateDefaultFolder (service: DriveService) =
    let folderName = "obsidian-sync"
    let request = service.Files.List()
    request.Q <- $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false"
    request.Fields <- "files(id)"
    let result = request.Execute()

    if result.Files.Count > 0 then
        result.Files.[0].Id
    else
        let metadata = Google.Apis.Drive.v3.Data.File(
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder"
        )
        let createRequest = service.Files.Create(metadata)
        createRequest.Fields <- "id"
        let folder = createRequest.Execute()
        printfn "Pasta padrão criada criada com ID: %s" folder.Id
        folder.Id

let UploadFile (service: DriveService, filePath: string, defaultFolder: string) =
    let fileMetadata = File(Name = Path.GetFileName(filePath), Parents = [| defaultFolder |])
    let mimeType = "application/octet-stream"

    use stream = new FileStream(filePath, FileMode.Open)
    let request = service.Files.Create(fileMetadata, stream, mimeType)
    request.Fields <- "id"

    let status = request.Upload() // only call once
    let uploadedFile = request.ResponseBody

    if status.Status <> Google.Apis.Upload.UploadStatus.Completed then
        failwithf "Upload falhou: %A" status.Exception

    printfn "✅ Arquivo enviado com sucesso! ID: %s" uploadedFile.Id
    Some uploadedFile.Id


let FileExistInCloud(service : DriveService, fileName: string) =
    let defaultFolder = GetOrCreateDefaultFolder(service)
    let request = service.Files.List()
    request.Q <- $"name = '{fileName}' and '{defaultFolder}' in parents and trashed = false"
    request.Fields <- "files(id)"
    let results = request.Execute()
    if results.Files.Count > 0 then Some results.Files.[0].Id else None

let UploadChangedFile (service: DriveService, filePath: string,fileName: string)=
    let fileExist = FileExistInCloud(service, fileName)
    use stream = new FileStream(filePath, FileMode.Open)
    let mimeType = "application/octet-stream"

    match fileExist with
    | Some fileId ->
        let updateRequest = service.Files.Update(null, fileId ,stream, mimeType)
        updateRequest.Fields <- "id"
        let status = updateRequest.Upload()
        let uploadedFile = updateRequest.ResponseBody

        if status.Status <> Google.Apis.Upload.UploadStatus.Completed then
            failwithf "Upload falhou: %A" status.Exception

        printfn "Arquivo atualizado com sucesso ID: %s" uploadedFile.Id
        Some uploadedFile.Id

    | None -> None

let recentPath = System.Collections.Concurrent.ConcurrentDictionary<string, DateTime>()

let ValidateUploadFiles(filePath: string) =
    let now = DateTime.UtcNow
    match recentPath.TryGetValue(filePath) with 
    | true, last when (now - last).TotalSeconds < 10.0 -> false
    | _ -> 
        recentPath.[filePath] <- now
        true