using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace G.Sync.Google.Api
{
    public class ApiContext
    {
        private static string _json;

        private static Lazy<ApiContext> _instance = new Lazy<ApiContext>(() => new ApiContext());
        public static ApiContext Instance
        {
            get
            {
                if (string.IsNullOrEmpty(_json))
                    throw new InvalidOperationException("JSON must be set before accessing ApiContext.Instance.");

                return _instance.Value;
            }
        }
        public static DriveService Connection { get; private set; }

        private ApiContext()
        {
            Initialize(_json);
        }

        public static void SetJson(string json)
        {
            _json = json;
            Initialize(_json);
        }

        private static void Initialize(string json)
        {

            Console.WriteLine("Passei aqui");

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { DriveService.Scope.Drive },
                "user",
                CancellationToken.None,
                new TokenContext()
            ).Result;

            Connection = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ObsidianGoogleDriveSyncPlugin"
            });
        }
    }
}
