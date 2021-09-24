using System.Net.Sockets;

namespace RevitSocketServer.Server
{
    internal class LastConnection
    {
        public TcpClient TcpClient { get; set; }
        public int ConnectionId { get; set; }
    }
}
