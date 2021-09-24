using RevitSocketServer.Server;
using SocketServerEntities.Entity;
using System.Threading.Tasks;

namespace RevitSocketServer.Handlers.ServerHandlers
{
    class DisconnectHandler : IHandler
    {
        public MsgType MsgType => MsgType.Disconnect;
        private readonly LastConnection _lastConnection;

        public DisconnectHandler(LastConnection lastConnection)
        {
            _lastConnection = lastConnection;
        }

        public Message Execute(Message msg)
        {
            throw new TaskCanceledException($"Разрыв соединения: {_lastConnection.TcpClient.Client.RemoteEndPoint}");
        }
    }
}
