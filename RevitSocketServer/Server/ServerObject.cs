using ExtensionLib.Util;
using RevitSocketServer.Handlers.RequestHandlers;
using RevitSocketServer.Handlers.ServerHandlers;
using RevitSocketServer.Util;
using SocketServerEntities.Entity;
using SocketServerEntities.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RevitSocketServer.Server
{
    public class ServerObject : IDisposable
    {
        private TcpListener _tcpListener = default;

        private readonly MessageDispatcher _messageDispatcher = new MessageDispatcher();
        private readonly LastConnection _lastConnection = new LastConnection();
        private readonly Dictionary<MsgType, IHandler> _serverHandlers = new Dictionary<MsgType, IHandler>();
        private readonly Dictionary<RequestType, IRequestTypeHandler> _requestTypeHandlers = new Dictionary<RequestType, IRequestTypeHandler>();
        private readonly SimpleDependencyContainer _dependencyContainer = new SimpleDependencyContainer();

        public List<T> InitHandlers<T>()
        {
            var types = typeof(T)
                .Assembly
                .DefinedTypes
                .Where(x => !x.IsAbstract && !x.IsInterface && typeof(T).IsAssignableFrom(x))
                .ToList();

            List<T> list = new List<T>();
            foreach (var t in types)
            {
                var ctor = t.GetConstructors().FirstOrDefault();
                if (!(ctor is null))
                {
                    List<object> prms = new List<object>();
                    foreach (var p in ctor.GetParameters())
                    {
                        prms.Add(_dependencyContainer.Get(p.ParameterType));
                    }
                    list.Add((T)ctor.Invoke(prms.ToArray()));
                }
            }
            return list;
        }

        public ServerObject(IDocumentManager manager)
        {
            _dependencyContainer.Add<IDocumentManager>(manager);
            _dependencyContainer.Add<MessageDispatcher>(_messageDispatcher);
            _dependencyContainer.Add<LastConnection>(_lastConnection);

            foreach (var item in InitHandlers<IHandler>())
            {
                _serverHandlers.Add(item.MsgType, item);
            }
            foreach (var item in InitHandlers<IRequestTypeHandler>())
            {
                _requestTypeHandlers.Add(item.RequestType, item);
            }
        }

        public void OnReceiveMessage(Message msg)
        {
            if (msg != default)
            {
                Logger.WriteInfo(
                    $"ip: {_lastConnection.TcpClient.Client.RemoteEndPoint}, " +
                    $"msg: \n{{\n\ttype:{msg.MsgType},\n\tguid:{msg.Guid},\n\tserviceKey:{msg.ServiceKey}\n}}"
                );
                if (_serverHandlers.ContainsKey(msg.MsgType))
                {
                    var result = _serverHandlers[msg.MsgType].Execute(msg);
                    if (!(result is null))
                    {
                        DataHandler.SendMessage(result, _lastConnection.TcpClient.GetStream());
                    }
                }
            }
        }
        public void RemoveConnection(int connectionId) =>
            _messageDispatcher.CancelConnection(connectionId);

        public void AddConnection(int connectionId, TcpClient client)
        {
            if (!(_lastConnection.TcpClient is null)) _lastConnection.TcpClient.Close();
            _lastConnection.TcpClient = client;
            _lastConnection.ConnectionId = connectionId;
            Task.Run(async () =>
            {
                var localClient = client;
                var id = connectionId;
                var ip = localClient.Client.RemoteEndPoint.ToString();
                try
                {
                    while (localClient.Connected)
                    {
                        DataHandler.GetMessage(localClient.GetStream());
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"Исключение = {ex.Message}");
                }
                finally
                {
                    RemoveConnection(id);
                    Logger.WriteError($"Разрыв соединения {ip}");
                }
            });
        }

        private async Task HandleMessage(MessageData item)
        {
            if (_requestTypeHandlers.ContainsKey(item.Message.RequestType))
            {
                var r = await _requestTypeHandlers[item.Message.RequestType]
                    .ExecuteAsync(item.Message, item.RequestCancellation.Token);
                if (!(r is null))
                {
                    DataHandler.SendMessage(r, item.TcpListner.GetStream());
                }
            }
        }

        private void StartHandlerOfIncomingMessages()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var item = _messageDispatcher.Pop();
                        if (item != default)
                        {
                            await HandleMessage(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError(ex.Message);
                    }
                    await Task.Delay(1000);
                }
            });
        }

        public async Task Listen(string adr = "127.0.0.1", int port = 11000)
        {
            try
            {
                Logger.Init("revit-server");
                IPAddress adress = IPAddress.Parse(adr);
                _tcpListener = new TcpListener(adress, 11000);
                _tcpListener.Start();
                StartHandlerOfIncomingMessages();
                DataHandler.OnReceive += OnReceiveMessage;

                Logger.WriteInfo($"Сервер запущен {adr}:{port} Ожидание подключений...");

                int connectionId = 0;
                while (true)
                {
                    TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                    if (!(tcpClient is null))
                    {
                        Logger.WriteInfo($"Соединение c {tcpClient.Client.RemoteEndPoint}");
                        AddConnection(connectionId++, tcpClient);
                    }
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.ToString());
                if (!(ex.InnerException is null))
                    Logger.WriteError(ex.InnerException.ToString());
            }
            finally
            {
                Logger.Dispose();
            }
        }

        public void Dispose()
        {
            _tcpListener.Stop();
            if (!(_lastConnection.TcpClient is null))
                _lastConnection.TcpClient.Close();
            Environment.Exit(0);
        }
    }
}
