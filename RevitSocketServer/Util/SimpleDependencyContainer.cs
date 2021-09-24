using SocketServerEntities.Exceptions;
using System;
using System.Collections.Generic;

namespace RevitSocketServer.Util
{
    public class SimpleDependencyContainer
    {

        private readonly Dictionary<Type, object> _objects = new Dictionary<Type, object>();

        public void Add<T>(object item)
        {
            _objects.Add(typeof(T), item);
        }

        public object Get(Type type)
        {
            if (_objects.ContainsKey(type))
            {
                return _objects[type];
            }
            throw new SimpleDependencyContainerNoTypeException($"no type {type}");
        }
    }
}
