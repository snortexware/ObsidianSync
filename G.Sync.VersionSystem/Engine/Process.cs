using Dapper;
using Database.Connection;
using G.Sync.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static G.Sync.DataContracts.VersionDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace G.Sync.VersionSystem.Engine
{
    public class Process
    {
        private readonly Dictionary<string, IVersion> _versions = [];

        private const string _versionExistSql =
            "SELECT STATUS FROM VERSIONS WHERE NAME = @name AND STATUS <> @appliedStatus";

        public void AddVersion(IVersion version, string name)
        {
            _versions.Add(name, version);
        }

        public void RunAll()
        {
            foreach (var version in _versions)
            {
                var versionName = version.Key;
                try
                {
                    var versionExist = VersionDontExistsOrExecuted(version);

                    if (versionExist)
                    {
                        version.Value.Run();
                        UpdateVersionStatus(versionName, StatusTypes.Applied);
                    }
                }
                catch (Exception ex)
                {
                    UpdateVersionStatus(versionName, StatusTypes.Failed);
                    throw new Exception($"Error executing version {version.Value}", ex);
                }
            }
        }

        private static bool VersionDontExistsOrExecuted(KeyValuePair<string, IVersion> version)
        {
            var cnn = GlobalDbConnection.Instance.Connection;

            var versionExist =
                cnn.Query<VersionDto>(_versionExistSql,
                new {name = version.Key, appliedStatus = StatusTypes.Applied} ).FirstOrDefault();

            if (versionExist == null)
            {
                var insertSql = "INSERT INTO VERSIONS (NAME, STATUS, CREATETIME) VALUES (@name, @status, @createdTime)";
                var parameters = new
                {
                    name = version.Key,
                    status = StatusTypes.Pending,
                    createdTime = version.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")
                };

                cnn.Execute(insertSql, parameters);

                return true;
            }

            return false;
        }

        private static void UpdateVersionStatus(string name, StatusTypes status)
        {
            UpdateVersionStatus(name, status, string.Empty);
        }

        private static void UpdateVersionStatus(string name, StatusTypes status, string error)
        {
            var cnn = GlobalDbConnection.Instance.Connection;

            var parameters = new DynamicParameters();
            parameters.Add("name", name);
            parameters.Add("status", status);

            var setClauses = new List<string> { "STATUS = @status" };
            if (!string.IsNullOrEmpty(error))
            {
                setClauses.Add("ERROR = @error");
                parameters.Add("error", error);
            }
            else
            {
                setClauses.Add("APPLIEDON = @appliedOn");
                parameters.Add("appliedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            var updateSql = $"UPDATE VERSIONS SET {string.Join(", ", setClauses)} WHERE NAME = @name";

            cnn.Execute(updateSql, parameters);
        }
    }
}
