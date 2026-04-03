using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace G.Sync.Google.Api
{
    public interface IGoogleDriveContext
    {
        Task<DriveService> GetConnectionAsync();
    }

    public class GoogleDriveContext : IGoogleDriveContext
    {
        private DriveService? _service;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public async Task<DriveService> GetConnectionAsync()
        {
            if (_service != null)
                return _service;

            await _lock.WaitAsync();
            try
            {
                if (_service != null)
                    return _service;

                var secrets = GetClientSecrets();

                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new[] { DriveService.Scope.DriveFile }, 
                    "user",
                    CancellationToken.None,
                    GetDataStore()
                );

                _service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ObsidianSync"
                });

                return _service;
            }
            finally
            {
                _lock.Release();
            }
        }

        private ClientSecrets GetClientSecrets()
        {
            var assembly = typeof(GoogleDriveContext).Assembly;

            using var stream = assembly.GetManifestResourceStream("G.Sync.Google.Api.google_oauth.json");

            return GoogleClientSecrets.FromStream(stream).Secrets;
        }

        private IDataStore GetDataStore()
        {
            var path = GetAppDataPath();
            return new FileDataStore(path, true);
        }

        private string GetAppDataPath()
        {
            string appName = "ObsidianSync";

            string basePath = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                PlatformID.Unix => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                _ => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            };

            var path = Path.Combine(basePath, appName, "auth");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}