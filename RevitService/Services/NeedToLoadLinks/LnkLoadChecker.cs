using RevitService.Services.GoogleServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Services.NeedToLoadLinks
{
    public class LnkLoadChecker : INeedToLoadLinks
    {
        private IGoogleRevitDocumentsProvider _googleRevitDocumentsProvider;
        public LnkLoadChecker(
            IGoogleRevitDocumentsProvider googleRevitDocumentsProvider)
        {
            _googleRevitDocumentsProvider = googleRevitDocumentsProvider;
        }

        public async Task<bool> LoadOrNotAsync(string name)
        {
            var filename = Path.GetFileNameWithoutExtension(name);
            var index = filename.IndexOf("BIM360");
            if (index >= 0)
                filename = filename.Substring(index + 6).Trim();
            var finded = (await _googleRevitDocumentsProvider.GetAsync())
                .FirstOrDefault(x => x.FileName.Contains(filename));
            return finded?.NeedToLoadLinks ?? false;
        }
    }
}
