using Newtonsoft.Json;
using SocketServerEntities.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Serializer
{
    class JsonWorker : ISerializer
    {
        public Message Deserialize(byte[] message)
        {
            var str = Encoding.UTF8.GetString(message);
            return JsonConvert.DeserializeObject<Message>(str);
        }

        public byte[] Serialize(Message message)
        {
            var result = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(result);
        }
    }
}
