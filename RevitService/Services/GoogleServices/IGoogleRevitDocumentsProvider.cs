using RevitService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevitService.Services.GoogleServices
{
    public interface IGoogleRevitDocumentsProvider
    {
        Task<List<GoogleRevitDocument>> GetAsync();
    }
}
