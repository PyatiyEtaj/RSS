using RevitService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SocketServerEntities.Util.DataHandler;

namespace RevitService.Services.RSO.Logic
{
    public interface IRSOService
    {
        public void Create(string authorization, string userId, string requestName, string downloadurn, string filePath, bool needToLoadLinks);
        public void CreateFromServerPath(string userId, string requestName, string serverPath, string filePath, bool needToLoadLinks);
        public Task<bool> CancelAsync(User user, string key);
        public Task<List<RevitRequest>> GetByUserIdAsync(string userId, int page);
        public Task<List<RevitRequest>> GetAsync(int page);
        public void OnReceive(CallBack callback);
    }
}
