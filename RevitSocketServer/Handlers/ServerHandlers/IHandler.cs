using SocketServerEntities.Entity;

namespace RevitSocketServer.Handlers.ServerHandlers
{
    interface IHandler
    {
        MsgType MsgType { get; }
        Message Execute(Message msg);
    }
}
