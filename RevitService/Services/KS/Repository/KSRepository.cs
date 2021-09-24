using MongoDB.Bson;
using MongoDB.Driver;
using RevitService.Models;
using RevitService.Services.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Repository
{
    public class KSRepository : IKSRepository
    {
        private readonly MongoService _mongo;

        public KSRepository(MongoService mongo) => _mongo = mongo;

        public async Task<KsDocument> FindByIdAsync(ObjectId id)
            => (await _mongo.Db.GetCollection<KsDocument>(MongoCollectionNames.KsDocuments.ToString()).FindAsync(x => x.Id.CompareTo(id) == 0)).FirstOrDefault();

        public async Task InsertDocumentAsync(KsDocument document)
        {
            await _mongo.Db.GetCollection<KsDocument>(MongoCollectionNames.KsDocuments.ToString()).InsertOneAsync(document);
        }

        public async Task UpdateDocumentFilePathChangedAsync(KsDocument original, string filepathchanged)
        {
            var res = await _mongo.Db.GetCollection<KsDocument>(MongoCollectionNames.KsDocuments.ToString())
                .UpdateOneAsync(
                    x => x.Id.CompareTo(original.Id) == 0,
                    Builders<KsDocument>.Update.Set("filepathchanged", filepathchanged)
                );
            var r = res;
        }
    }
}
