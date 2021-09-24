using Microsoft.Extensions.Logging;
using RevitService.Models;
using System.Threading;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Logic
{
    public class KsByRevitService : AKsService
    {
        private readonly ILogger<KsByRevitService> _logger;
        private readonly ClientTcpObject _clientTcp;

        public KsByRevitService(ILogger<KsByRevitService> logger, ClientTcpObject clientTcp)
        {
            _logger = logger;
            _clientTcp = clientTcp;
        }
        public override async Task<bool> CreateAsync(string filepath, string authorization, KsDTO dto, CancellationToken cancellationToken = default)
        {
            /*if (await GetGoogleDocAsync())
            {
                var msg = new SocketServerEntities.Entity.Message
                {
                    Body = new
                    {
                        path = "KsExtension.dll KsExtension.Main",
                        authorization,
                        dto.downloadurn,
                        dto.urn,
                        dto.start,
                        dto.end,
                        excelDocData = Convert.ToBase64String(GetDocumentData())
                    },
                    MsgType = SocketServerEntities.Entity.MsgType.Method,
                    //RequireReturn = true,
                    Guid = Guid.NewGuid()
                };
                _clientTcp.Request(msg);
                if (msg.RequireReturn)
                {
                    var result = await _clientTcp.ResponseAsync(msg.Guid, cancellationToken);
                    if (result?.MsgType == SocketServerEntities.Entity.MsgType.Error)
                    {
                        throw new RevitServerException(msg.Body.ToString());
                    }
                }
            }
            return true;*/
            return await Task.FromResult(true);
        }
    }
}
