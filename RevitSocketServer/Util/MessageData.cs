using SocketServerEntities.Entity;
using System.Net.Sockets;
using System.Threading;

namespace RevitSocketServer.Util
{
    public class MessageData
    {
        public Message Message { get; set; }
        public TcpClient TcpListner { get; set; }
        public CancellationTokenSource RequestCancellation { get; set; }
        public int ConnectionId { get; set; }
        //public bool IsCancelled { get; set; } = false;
    }
}
