using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Exceptions
{
    public class DocumentLoaderException : Exception
    {
        public DocumentLoaderException(string message) : base(message)
        {
        }
    }
}
