namespace Google.Api.MainProcess.TokenContext

open Database.Connection
open Google.Apis.Util.Store

open System
open System.Threading.Tasks
open Google.Apis.Util.Store
open Microsoft.Data.Sqlite
open System.Text.Json

type DbContextToken() =
    interface IDataStore with
        member _.StoreAsync<'T>(key: string, value: 'T) : Task =
            task {
                let tokenJson = JsonSerializer.Serialize<'T>(value)
                let now = DateTime.UtcNow.ToString("o")
                let conn = GlobalDbConnection.Instance.Connection
                use cmd = conn.CreateCommand()
                cmd.CommandText <- """
                    INSERT INTO SECURITY (KEY, TOKEN,  CREATEDAT)
                    VALUES (@key, @token, @created)
                    ON CONFLICT(KEY) DO UPDATE SET
                        TOKEN = excluded.TOKEN,
                        CREATEDAT = excluded.CREATEDAT;
                """
                cmd.Parameters.AddWithValue("@key", key) |> ignore
                cmd.Parameters.AddWithValue("@token", tokenJson) |> ignore
                cmd.Parameters.AddWithValue("@created", now) |> ignore
                let! _ = cmd.ExecuteNonQueryAsync()
                return ()
            }

        member _.GetAsync<'T>(key: string) : Task<'T> =
            task {
                let conn = GlobalDbConnection.Instance.Connection
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "SELECT TOKEN FROM SECURITY WHERE KEY = @key"
                cmd.Parameters.AddWithValue("@key", key) |> ignore
                use reader = cmd.ExecuteReader()
                if reader.Read() then
                    let json = reader.GetString(0)
                    return JsonSerializer.Deserialize<'T>(json)
                else
                    return Unchecked.defaultof<'T>
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