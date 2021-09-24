
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SocketServerEntities.Entity;
using System;
using System.IO;

namespace SocketServerEntities.Serializer
{
    public class BsonWorker : ISerializer
    {

        public Message Deserialize(byte[] message)
        {
            MemoryStream ms = new MemoryStream(message);
            using (var reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<Message>(reader);
            }
        }

        public byte[] Serialize(Message message)
        {
            MemoryStream ms = new MemoryStream();
            using (var writer = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, message);
            }

            return ms.ToArray();
        }
    }
}
