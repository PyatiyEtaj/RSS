using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Exceptions
{
    public class DocumentManagerException : Exception
    {
        public DocumentManagerException(string message) : base(message)
        {
        }
    }
}
