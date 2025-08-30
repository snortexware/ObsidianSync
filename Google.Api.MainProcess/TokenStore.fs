namespace Google.Api.MainProcess.TokenContext

open Database.Connection
open Google.Apis.Util.Store

open System
open System.Threading.Tasks
open Google.Apis.Util.Store
open System.Text.Json
open G.Sync.Repository

type DbContextToken() =
    interface IDataStore with
        member _.StoreAsync<'T>(key: string, value: 'T) : Task =
            task {
                let tokenJson = JsonSerializer.Serialize<'T>(value)
                let repo = new SecurityRepository();

                repo.CreateOrUpdateToken(key, tokenJson);

                return ()
            }

        member _.GetAsync<'T>(key: string) : Task<'T> =
            task {
                    let repo = new SecurityRepository();
                
                    let token = repo.GetTokenByKey(key) 

                    return JsonSerializer.Deserialize<'T>(token.Token);
                }

        member _.DeleteAsync(key: string) : Task =
            task {
                let conn = GlobalDbConnection.Instance.Connection
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "DELETE FROM SECURITY WHERE KEY = @key"
                cmd.Parameters.AddWithValue("@key", key) |> ignore
                let! _ = cmd.ExecuteNonQueryAsync()
                return ()
            }

        member _.ClearAsync() : Task =
            task {
                let conn = GlobalDbConnection.Instance.Connection
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "DELETE FROM SECURITY"
                let! _ = cmd.ExecuteNonQueryAsync()
                return ()
            }