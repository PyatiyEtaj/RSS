using Microsoft.Extensions.Logging;
using SocketServerEntities.Entity;
using SocketServerEntities.Exceptions;
using SocketServerEntities.Util;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RevitService.Services
{
    public class ClientTcpObject : IDisposable
    {
        public string Name { get; private set; }
        public NetworkStream Stream { get; private set; } = default;
        private readonly TcpClient _client = new();

        private readonly ILogger<ClientTcpObject> _logger;

        public Dictionary<Guid, Message> MessagesFromRSS { get; private set; } = new();

        public ClientTcpObject(ILogger<ClientTcpObject> logger, string adress, int port)
        {
            _logger = logger;

            DataHandler.OnReceive += (msg) =>
            {
                _logger.LogInformation($"Ответ от RSS\n\t{msg}");
            };

            if (TryToConnect(adress, port))
            {
                Task.Run(HandleResponseFromRSS);
            }
        }

        public bool TryToConnect(string adr, int port)
        {
            _logger.LogInformation("Попытка соединения ...");
            try
            {
                _client.Connect(adr, port);
                Stream = _client.GetStream();
                _logger.LogInformation("Соединение установлено");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось соединиться с {adr}:{port}\n\tОшибка = {ex.Message}");
                return false;
            }
            return _client.Connected;
        }

        private async Task HandleResponseFromRSS()
        {
            while (_client?.Connected == true)
            {
                DataHandler.GetMessage(Stream);
                await Task.Delay(500);
            }
        }

        public void Request(Message message)
        {
            if (!_client.Connected)
                throw new RevitServerNotConnected("[ClientTcpObject.RequestRSS] Нет соединения");

            DataHandler.SendMessage(message, Stream);
        }

        public void OnReceive(string serviceKey, DataHandler.CallBack onReceive)
        {
            DataHandler.OnReceive += (msg) =>
            {
                if (msg.ServiceKey == serviceKey)
                {
                    onReceive(msg);
                }
            };
        }

        public async Task<Message> ResponseAsync(Guid guid, CancellationToken cancellationToken)
        {
            while (_client?.Connected == true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Request(new Message()
                    {
                        Guid = guid,
                        MsgType = MsgType.Cancel
                    });
                    cancellationToken.ThrowIfCancellationRequested();
                }
                if (MessagesFromRSS.TryGetValue(guid, out var result))
                {
                    MessagesFromRSS.Remove(guid);
                    return result;
                }
                await Task.Delay(500);
            }
            return null;
        }

        private void Close()
        {
            if (Stream != null)
            {
                DataHandler.SendMessage(new Message { MsgType = MsgType.Disconnect }, Stream);
                Stream.Close();
            }
            _client?.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
