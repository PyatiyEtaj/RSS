using RevitSocketServer.Util;
using SocketServerEntities.Entity;

namespace RevitSocketServer.Handlers.ServerHandlers
{
    internal class CancelHandler : IHandler
    {
        public MsgType MsgType => MsgType.Cancel;

        private readonly MessageDispatcher _messageDispatcher;

        public CancelHandler(MessageDispatcher messageDispatcher)
        {
            _messageDispatcher = messageDispatcher;
        }

        public Message Execute(Message msg)
        {
            if (!_messageDispatcher.CancelRequest(msg.Guid))
            {
                var resultMsg = new Message
                {
                    Body = "Отменен",
                    MessageResult = MessageResult.Canceled,
                    Guid = msg.Guid,
                    ServiceKey = msg.ServiceKey
                };
                return resultMsg;
            }
            return default;
        }
    }
}
