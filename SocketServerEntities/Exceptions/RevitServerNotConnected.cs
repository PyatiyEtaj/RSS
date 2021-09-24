using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Exceptions
{
    public class RevitServerNotConnected : Exception
    {
        public RevitServerNotConnected(string message) : base(message)
        {
        }
    }
}
