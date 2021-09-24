using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Exceptions
{
    public class SimpleDependencyContainerNoTypeException : Exception
    {
        public SimpleDependencyContainerNoTypeException(string message) : base(message)
        {
        }
    }
}
