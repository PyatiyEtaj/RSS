using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;
using RevitService.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RevitService.Services.GoogleServices
{
    public class GoogleSheets : IGoogleRevitDocumentsProvider
    {
        private readonly SheetsService _service;

        private static Google.Apis.Drive.v3.Data.File _dbInfo = null;

        public string ApplicationName { get; } = "Revit pco files";
        public string SheetId { get; set; }
        public List<string> Ranges { get; set; }

        private List<GoogleRevitDocument> _revitFiles = new();

        private GoogleDriveService _googleDriveService;

        public GoogleSheets(IGoogleCredentials credentials, GoogleDriveService googleDriveService,  string sheetsId, List<string> ranges)
        {
            SheetId = sheetsId;
            Ranges = ranges;

            _googleDriveService = googleDriveService;

            _service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credentials.Credential,
                ApplicationName = ApplicationName,
            });
        }

        private async Task<bool> NeedUpdateAsync()
        {
            var info = await _googleDriveService.Set(SheetId).GetMetaInfoAsync();
            var needUpdate = (_dbInfo is null || _dbInfo.ModifiedTime != info.ModifiedTime);
            _dbInfo = info;
            return needUpdate;
        }

        public async Task<List<GoogleRevitDocument>> GetAsync()
        {
            if (!(await NeedUpdateAsync())) return _revitFiles;
            
            _revitFiles.Clear();
            foreach (var range in Ranges)
            {
                // var objectName = range.Split('!').FirstOrDefault();
                var response = _service.Spreadsheets
                    .Values.Get(SheetId, range).Execute();

                for (int i = 1; i < response.Values.Count; i++)
                {
                    var row = response.Values[i];
                    if (row.Count < 5) continue;
                    _revitFiles.Add(new GoogleRevitDocument
                    {
                        ObjectName = row[0].ToString().Trim(),
                        Devide = row[1].ToString().Trim(),
                        Section = row[2].ToString().Trim(),
                        Name = row[3].ToString().Trim(),
                        Information = row[4].ToString(),
                        NeedToLoadLinks = row[5].ToString() == "да",
                        Path = row[6].ToString().Trim(),
                        FileName = Path.GetFileNameWithoutExtension(row[6].ToString().Replace('_', ' ')).Trim(),
                    });
                }
            }

            return _revitFiles;
        }
    }
}
