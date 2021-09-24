using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Exceptions
{
    public class RevitServerException : Exception
    {
        public RevitServerException(string message) : base(message)
        {
        }
    }
}
