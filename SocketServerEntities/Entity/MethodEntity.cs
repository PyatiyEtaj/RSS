using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Entity
{
    public class MethodEntity
    {
        public string AssemblyName { get; set; }
        public string Type { get; set; }
        public string MethodName { get; set; }
        public object[] Args { get; set; }

        public override string ToString()
            => $"AssemblyName {AssemblyName} | Type {Type} | MethodName {MethodName}";
    }
}
