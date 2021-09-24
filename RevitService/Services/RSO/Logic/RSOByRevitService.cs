using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RevitService.Models;
using RevitService.Providers;
using RevitService.Services.Repository;
using RevitService.SignalR.Hubs;
using SocketServerEntities.Entity;
using SocketServerEntities.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RevitService.Services.RSO.Logic
{
    public class RSOByRevitService : IRSOService
    {
        private readonly ILogger<RSOByRevitService> _logger;
        private readonly ClientTcpObject _clientTcp;
        private readonly IRevitRequestsRepository _repository;
        private readonly string _serviceKey;
        private readonly IHubContext<RSOHub> _hubContext;

        public RSOByRevitService(
            ILogger<RSOByRevitService> logger,
            ClientTcpObject clientTcp,
            IRevitRequestsRepository repository,
            IServiceKeyProvider serviceKeyProvider, 
            IHubContext<RSOHub> hubContext)
        {
            _logger = logger;
            _clientTcp = clientTcp;
            _repository = repository;
            _serviceKey = serviceKeyProvider.Next();

            OnReceive(async (msg) =>
            {
                await _repository.UpdateRequestInformationAsync(msg.Guid, msg.Body.ToString(), msg.MessageResult);
            });

            _repository.OnInsertUpdateDeleteEvent += async (item) =>
            {
                await _hubContext.Clients.All.SendAsync("OnRSSReceiveMessage", item);
            };

            _hubContext = hubContext;
        }


        public async Task<bool> CancelAsync(User user, string key)
        {
            var item = await _repository.GetSpecificByUserIdAsync(user.UserId, key);
            if (item is not null)
            {
                await _repository.UpdateRequestInformationAsync(
                    item.Key,
                    "Ожидаем отмены запроса со стороны RSS",
                    MessageResult.AwaitingCancellation
                );
                _clientTcp.Request(
                    new SocketServerEntities.Entity.Message
                    {
                        Guid = item.Key,
                        MsgType = SocketServerEntities.Entity.MsgType.Cancel,
                        ServiceKey = _serviceKey
                    }
                );
                return true;
            }
            return false;
        }

        public void Create(string authorization, string userId, string requestName, string downloadurn, string filePath, bool needToLoadLinks)
        {
            var key = Guid.NewGuid();
            var msg = new SocketServerEntities.Entity.Message
            {
                Body = new
                {
                    path = $"{Path.Combine("RSO", "RSOForm.dll")} RSO.RSOForWeb",
                    authorization,
                    downloadurn,
                    filePath,
                    needToLoadLinks
                },
                MsgType = MsgType.PushRequest,
                RequestType = RequestType.ExecuteRevitExtension,
                Guid = key,
                ServiceKey = _serviceKey
            };
            _clientTcp.Request(msg);
            _repository.Insert(new RevitRequest
            {
                UserId = userId,
                Date = DateTime.Now,
                Name = requestName,
                RequestType = RevitRequestsType.Rso.ToString(),
                Status = MessageResult.InProgress.ToString(),
                Key = key,
                DocumentPath = Path.GetFileName(filePath),
                ServerResponse = ""
            });
        }

        public void CreateFromServerPath(string userId, string requestName, string serverPath, string filePath, bool needToLoadLinks)
        {
            var key = Guid.NewGuid();
            var msg = new SocketServerEntities.Entity.Message
            {
                Body = new
                {
                    path = $"{Path.Combine("RSO", "RSOForm.dll")} RSO.RSOForWeb",
                    serverPath,
                    filePath,
                    needToLoadLinks
                },
                MsgType = MsgType.PushRequest,
                RequestType = RequestType.ExecuteRevitExtension,
                Guid = key,
                ServiceKey = _serviceKey,
            };
            _clientTcp.Request(msg);
            _repository.Insert(new RevitRequest
            {
                UserId = userId,
                Date = DateTime.Now,
                Name = requestName,
                RequestType = RevitRequestsType.Rso.ToString(),
                Status = MessageResult.InProgress.ToString(),
                Key = key,
                DocumentPath = Path.GetFileName(filePath),
                ServerResponse = ""
            });
        }

        public Task<List<RevitRequest>> GetByUserIdAsync(string userId, int page)
        {
            return _repository.GetByUserIdAsync(userId, page);
        }

        public Task<List<RevitRequest>> GetAsync(int page)
        {
            return _repository.GetAsync(page);
        }

        public void OnReceive(DataHandler.CallBack callback)
        {
            _clientTcp.OnReceive(_serviceKey, callback);
        }
    }
}
