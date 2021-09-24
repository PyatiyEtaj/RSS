using SocketServerEntities.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServerClient.Server;

namespace TestServerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string adr = "127.0.0.1";
            int port = 11000;
            Task.Run(async () =>
            {
                using (var serv = new RevitSocketServer.Server.ServerObject(null))
                {
                    await serv.Listen(adr, port);
                }
            });
            Task.Run(() =>
            {
                using (var client = new ClientObject())
                {
                    if (client.TryToConnect(adr, port))
                    {
                        client.Process();
                    }
                }
            });
            Console.ReadLine();
        }
    }
}
