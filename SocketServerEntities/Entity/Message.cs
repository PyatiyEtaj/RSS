using System;

namespace SocketServerEntities.Entity
{
    public class Message
    {
        public object Body { get; set; }

        public MsgType MsgType { get; set; }
        public RequestType RequestType { get; set; }
        public MessageResult MessageResult { get; set; }

        public Guid Guid { get; set; }

        // Используется сервисами для определения своих запросов
        public string ServiceKey { get; set; }

        public override string ToString() 
            => "{{\n\t" +
            $"Body: {{{Body}}};\n\t" +
            $"MsgType: {{{MsgType}}};\n\t" +
            $"Guid: {{{Guid}}}\n\t" +
            $"MessageResult: {{{MessageResult}}}\n\t" +
            $"RequestType: {{{RequestType}}}\n\t" +
            $"ServiceKey: {{{ServiceKey}}}\n\t" +
            "}}";
    }
}
