using ExtensionLib.Entity;
using Google.Apis.Download;
using RevitService.Models;
using RevitService.Services.GoogleServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Logic
{
    public abstract class AKsService
    {
        private static Google.Apis.Drive.v3.Data.File _dbInfo = null;
        private const string _docIdPrices = "1PO-D_5DW9pmE1LXU_rSnQ2EYMUch3UngfzIwx9NyPNw";
        private readonly string _docPath = FilePathes.ExcelPrices;

        protected async Task<bool> GetGoogleDocAsync(GoogleDriveService service)
        {
            service.Set(_docIdPrices);
            bool result = true;
            var info = await service.GetMetaInfoAsync();
            if (_dbInfo is null || _dbInfo.ModifiedTime != info.ModifiedTime) // или Version 2403 или ThumbnailVersion 1567
            {
                var progress = await service.DownloadExcelAsync(_docPath);
                result = progress.Status == DownloadStatus.Completed;
            }
            _dbInfo = info;
            return result;
        }

        protected byte[] GetDocumentData()
            => File.ReadAllBytes(_docPath);

        public abstract Task<bool> CreateAsync(string filepath, string authorization, KsDTO dto, CancellationToken cancellationToken = default);
    }
}
