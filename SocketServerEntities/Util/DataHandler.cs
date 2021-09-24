using SocketServerEntities.Entity;
using SocketServerEntities.Serializer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServerEntities.Util
{
    public static class DataHandler
    {
        public delegate void CallBack(Message message);
        public static event CallBack OnReceive;
        public const int BUFFER_SIZE = 256;
        private static ISerializer _worker = new JsonWorker();
        private static readonly int INT_SIZE = sizeof(int);

        private static SpinLock _slSend = new SpinLock();
        private static SpinLock _slRead = new SpinLock();

        private static byte[] ToBytes(Message msg)
            => _worker.Serialize(msg);

        public static void SendMessage(Message msg, NetworkStream stream)
        {
            _slSend.Lock();
            try
            {
                var data = ToBytes(msg);
                int length = data.Length;
                if (length > 0)
                {
                    var bytes = BitConverter.GetBytes(length);
                    stream.Write(bytes, 0, INT_SIZE);
                    for (int i = 0; i < data.Length; i += BUFFER_SIZE)
                    {
                        stream.Write(data, i, length > BUFFER_SIZE ? BUFFER_SIZE : length);
                        length -= BUFFER_SIZE;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _slSend.Unlock();
            }
        }

        private static int ReadLength(NetworkStream stream)
        {
            var length = new byte[INT_SIZE];
            var readed = stream.Read(length, 0, INT_SIZE);
            return readed > 0 ? BitConverter.ToInt32(length, 0) : -1;
        }

        public static Message GetMessage(NetworkStream stream)
        {
            _slRead.Lock();
            try
            {
                if (!stream.DataAvailable) return default;

                int getLength = ReadLength(stream);
                if (getLength > 0)
                {
                    var data = new byte[getLength];
                    int bytes = 0;
                    while (bytes < getLength)
                    {
                        if (stream.DataAvailable)
                            bytes += stream.Read(data, bytes, getLength-bytes);
                        else
                            Thread.Sleep(10);
                    }
                    var resultData = bytes > 0 ? _worker.Deserialize(data) : default;
                    if (resultData != default)
                        OnReceive?.Invoke(resultData);
                    return resultData;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _slRead.Unlock();
            }
            return null;
        }
    }
}
