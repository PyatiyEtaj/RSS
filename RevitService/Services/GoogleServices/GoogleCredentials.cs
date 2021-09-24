using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using RevitService.Properties;

namespace RevitService.Services.GoogleServices
{
    class GoogleCredentials : IGoogleCredentials
    {
        private Google.Apis.Auth.OAuth2.GoogleCredential _credentials = null;
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets, DriveService.Scope.DriveReadonly, DriveService.Scope.DriveMetadataReadonly };
        public Google.Apis.Auth.OAuth2.GoogleCredential Credential => _credentials;

        public GoogleCredentials()
        {
            using (var stream = new MemoryStream(Resources.service_secret))
            {
                _credentials = GoogleCredential.FromStream(stream)
                    .CreateScoped(_scopes);
            }

            if (_credentials is null) throw new Exception("Cannot create GoogleCredentials");
        }
    }
}
