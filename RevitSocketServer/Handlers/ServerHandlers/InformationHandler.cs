using RevitSocketServer.Util;
using SocketServerEntities.Entity;

namespace RevitSocketServer.Handlers.ServerHandlers
{
    internal class InformationHandler : IHandler
    {
        public MsgType MsgType => MsgType.GetInformation;

        private readonly MessageDispatcher _dispatcher;

        public InformationHandler(MessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public Message Execute(Message msg)
        {
            var i = _dispatcher.RequestPosition(msg.Guid);
            var position = i == -1 ? $"Позиция {i}" : "Запрос не найден";
            return new Message
            {
                Body = position,
                MessageResult = MessageResult.Success,
                ServiceKey = msg.ServiceKey,
                Guid = msg.Guid
            };
        }
    }
}
