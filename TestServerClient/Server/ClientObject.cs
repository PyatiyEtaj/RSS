using SocketServerEntities.Entity;
using SocketServerEntities.Serializer;
using SocketServerEntities.Util;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TestServerClient.Server
{
    class ClientObject : IDisposable
    {        
        public string Name { get; private set; }
        public NetworkStream Stream { get; private set; } = default;
        private TcpClient _client = new System.Net.Sockets.TcpClient();
        private ISerializer _worker = new BsonWorker();

        public bool TryToConnect(string adr, int port)
        {
            Console.WriteLine("Попытка соединения");
            try
            {
                _client.Connect(adr, port);
                Stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot connect to {adr}:{port}\n\terror = {ex.Message}");
                return false;
            }
            return _client.Connected;
        }

        public void Process()
        {
            try
            {
                //while (true)
                {
                    try
                    {
                        /*var message = DataHandler.GetMessage(Stream);
                        if (message != default)
                        {
                            Console.WriteLine($"полученно : {message}");                            
                        }
                        string rawtext = Console.ReadLine();
                        if (rawtext == @"/END")
                            break;
                        if (rawtext == @"/TEST")
                        {
                            rawtext = GetRandomString(5000);
                            Console.WriteLine(rawtext);
                        }
                        DataHandler.SendMessage(new Message()
                        {
                            Body = RSSConverter.ToByteArray(rawtext),
                            MsgType = MsgType.Msg
                        }, Stream);*/
                        DataHandler.SendMessage(new SocketServerEntities.Entity.Message
                        {
                            Body = new
                            {
                                path = "KsGenerator.dll KsGenerator.Main Execute",
                                urn = "dto.urn",
                                downloadurn = "dto.downloadurn",
                                start = DateTime.Now,
                                end = DateTime.Now,
                                auth = "asdasdasdas"
                            },
                            MsgType = SocketServerEntities.Entity.MsgType.Method
                        }, Stream);
                        while (true)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        //break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private string GetRandomString(int v)
        {
            Random rd = new Random();
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            StringBuilder s = new StringBuilder(v+1);
            for (int i = 0; i < v; i++)
            {
                s.Append(allowedChars[rd.Next(0, allowedChars.Length)]);
            }
            return s.ToString();
        }

        private void Close()
        {
            if (Stream != null)
            {
                DataHandler.SendMessage(new Message { MsgType = MsgType.Disconnect }, Stream);
                Stream.Close();
            }
            if (_client != null)
                _client.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
