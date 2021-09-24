using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Exceptions
{
    public class GetBim360FileException : Exception
    {
        public GetBim360FileException(string message) : base(message)
        {
        }
    }
}
