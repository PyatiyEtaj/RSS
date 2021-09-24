using RevitService.Models;
using SocketServerEntities.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevitService.Services.Repository
{
    public delegate void OnInsertUpdateDelete(RevitRequest request);

    public interface IRevitRequestsRepository
    {
        Task<List<RevitRequest>> GetByUserIdAsync(string userId, int page);
        Task<RevitRequest> GetSpecificByUserIdAsync(string userId, string id);
        Task<List<RevitRequest>> GetAsync(int page);
        Task<RevitRequest> GetByGuidKeyAsync(Guid key);
        void Insert(RevitRequest request);
        Task<RevitRequest> UpdateRequestInformationAsync(Guid key, string serverResponse, MessageResult status);
        event OnInsertUpdateDelete OnInsertUpdateDeleteEvent;
    }
}
