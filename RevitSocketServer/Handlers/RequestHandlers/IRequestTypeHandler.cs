using SocketServerEntities.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace RevitSocketServer.Handlers.RequestHandlers
{
    internal interface IRequestTypeHandler
    {
        RequestType RequestType { get; }

        Task<Message> ExecuteAsync(Message message, CancellationToken token);
    }
}
