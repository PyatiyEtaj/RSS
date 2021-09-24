using MongoDB.Bson;
using RevitService.Models;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Repository
{
    public interface IKSRepository
    {
        Task InsertDocumentAsync(KsDocument document);
        Task<KsDocument> FindByIdAsync(ObjectId id);

        Task UpdateDocumentFilePathChangedAsync(KsDocument original, string filepathchanged);
    }
}
