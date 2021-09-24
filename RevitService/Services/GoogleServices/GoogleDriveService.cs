using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.IO;
using System.Threading.Tasks;

namespace RevitService.Services.GoogleServices
{
    public class GoogleDriveService
    {
        public string ApplicationName { get; } = "Google Drive API .NET Downloader";

        private readonly DriveService _service = default;
        private string _docId = default;
        public readonly string _secret = Path.Combine("docs", "service-secret.json");

        public GoogleDriveService(IGoogleCredentials credentials)
        {
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials.Credential,
                ApplicationName = ApplicationName,
            });
        }

        public GoogleDriveService Set(string docId)
        {
            _docId = docId;
            return this;
        }

        public async Task<Google.Apis.Drive.v3.Data.File> GetMetaInfoAsync()
        {
            if (_docId != default && _service != default)
            {
                var req = _service.Files.Get(_docId);
                req.Fields = "*";
                req.SupportsAllDrives = true;
                return await req.ExecuteAsync();
            }
            return default;
        }

        public async Task<IDownloadProgress> DownloadExcelAsync(string outFileName)
        {
            if (_docId == default || _service == default)
                return default;

            IDownloadProgress result = null;
            using (var output = new FileStream(outFileName, FileMode.OpenOrCreate))
            {
                result = await _service.Files
                    .Export(_docId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    .DownloadAsync(output);
            }
            return result;
        }
        public IDownloadProgress DownloadExcel(string outFileName)
        {
            if (_docId == default || _service == default)
                return default;

            IDownloadProgress result = null;
            using (var output = new FileStream(outFileName, FileMode.OpenOrCreate))
            {
                result = _service.Files
                    .Export(_docId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    .DownloadWithStatus(output);
            }
            return result;
        }
    }
}
