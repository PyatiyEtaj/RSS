
using SocketServerEntities.Entity;

namespace SocketServerEntities.Serializer
{
    public interface ISerializer
    {
        Message Deserialize(byte[] message);

        byte[] Serialize(Message message);
    }
}
