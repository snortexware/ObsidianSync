module UserConnection

open Google.Apis.Auth.OAuth2
open Google.Apis.Drive.v3
open Google.Apis.Services
open Google.Apis.Util.Store
open System
open System.IO
open System.Threading
open Google.Api.MainProcess.TokenContext

type ApiConnection (appName: string) =
    let scopes = [| DriveService.Scope.Drive |]
    let clientSecret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "client_secret.json")

    let credential =
                    async {
                            use stream = new FileStream(clientSecret, FileMode.Open, FileAccess.Read)
                            return! GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            scopes,
                            "user",
                            CancellationToken.None,
                            DbContextToken() :> IDataStore
                                ) |> Async.AwaitTask
                            } |> Async.RunSynchronously

    let service = new DriveService(
                                   BaseClientService.
                                    Initializer(
                                    HttpClientInitializer = credential,
                                    ApplicationName = appName
                                    ))

    static let mutable instance: Lazy<ApiConnection> option = None

    static member Init(appName: string) = 
           if instance.IsNone then
            instance <- Some (lazy (ApiConnection(appName)))

    static member Instance =
        match instance with
        | Some inst -> inst.Value
        | None -> failwith "A conexão com a api não foi iniciada."

    member _.Service = service
