using MongoDB.Bson;
using MongoDB.Driver;
using RevitService.Models;
using SocketServerEntities.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Services.Repository
{
    public class RevitRequestsRepository : IRevitRequestsRepository
    {
        private readonly MongoService _mongo;
        private const int _maxDocumentsInOneResponse = 12;

        public event OnInsertUpdateDelete OnInsertUpdateDeleteEvent;

        public RevitRequestsRepository(MongoService mongo) => _mongo = mongo;

        public async Task<RevitRequest> UpdateRequestInformationAsync(Guid key, string serverResponse, MessageResult status)
        {
            var collection = _mongo.Db.GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString());
            var item = await collection.FindOneAndUpdateAsync(
                x => x.Key == key,
                Builders<RevitRequest>.Update
                    .Set("status", status.ToString())
                    .Set("serverResponse", serverResponse)
            );
            item.Status = status.ToString();
            item.ServerResponse = serverResponse;
            OnInsertUpdateDeleteEvent?.Invoke(item);
            return item;
        }

        public void Insert(RevitRequest request)
        {
            _mongo.Db.GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString()).InsertOne(request);
            OnInsertUpdateDeleteEvent?.Invoke(request);
        }


        public async Task<List<RevitRequest>> GetByUserIdAsync(string userId, int page)
        {
            return await _mongo.Db
                .GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString())
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.Date)
                .Skip((page - 1) * _maxDocumentsInOneResponse)
                .Limit(_maxDocumentsInOneResponse)
                .ToListAsync();           
        }

        public async Task<List<RevitRequest>> GetAsync(int page)
        {
            return await _mongo.Db
                .GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString())
                .Find(x => true)
                .SortByDescending(x => x.Date)
                .Skip((page - 1) * _maxDocumentsInOneResponse)
                .Limit(_maxDocumentsInOneResponse)
                .ToListAsync();
        }

        public async Task<RevitRequest> GetSpecificByUserIdAsync(string userId, string id)
        {
            ObjectId objectId = new ObjectId(id);
            var result = await _mongo.Db
                .GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString())
                .FindAsync(x => x.UserId == userId && x.Id == objectId);
            return result.FirstOrDefault();
        }

        public async Task<RevitRequest> GetByGuidKeyAsync(Guid key)
        {
            var result = await _mongo.Db
                .GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString())
                .FindAsync(x => x.Key == key);
            return result.FirstOrDefault();
        }
    }
}
