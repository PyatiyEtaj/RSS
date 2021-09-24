using RevitSocketServer.Server;
using RevitSocketServer.Util;
using SocketServerEntities.Entity;
using System.Threading;

namespace RevitSocketServer.Handlers.ServerHandlers
{
    internal class PushRequestHandler : IHandler
    {
        public MsgType MsgType => MsgType.PushRequest;
        private readonly MessageDispatcher _messageDispatcher;
        private readonly LastConnection _lastConnection;

        public PushRequestHandler(MessageDispatcher messageDispatcher, LastConnection lastConnecton)
        {
            _lastConnection = lastConnecton;
            _messageDispatcher = messageDispatcher;
        }

        public Message Execute(Message msg)
        {
            var args = (Newtonsoft.Json.Linq.JObject)msg.Body;
            var uid = (string)args["downloadurn"];
            _messageDispatcher.Push(
                uid,
                new MessageData
                {
                    Message = msg,
                    TcpListner = _lastConnection.TcpClient,
                    RequestCancellation = new CancellationTokenSource(),
                    ConnectionId = _lastConnection.ConnectionId
                }
            );

            return default;
        }
    }
}
