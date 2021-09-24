using ExtensionLib.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RevitSocketServer.Util
{
    public class AssemblyLoader : IPlugin
    {
        private readonly Dictionary<string, Assembly> _loadedAsms = new Dictionary<string, Assembly>();
        public static readonly string AssemblyPossibleDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk", "Revit", "Addins", "2019", "RevitSocketServer");

        public AssemblyLoader()
        {
            string p = Path.Combine(AssemblyPossibleDir, "files");
            string ext = Path.Combine(AssemblyPossibleDir, "extensions");
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            if (!Directory.Exists(ext))
                Directory.CreateDirectory(ext);
        }

        public Assembly Load(string asmname)
        {
            //if (_loadedAsms.ContainsKey(asmname))
            //    return _loadedAsms[asmname];
            var asm = Assembly.LoadFrom(Path.Combine(AssemblyPossibleDir, "extensions", asmname));
            //_loadedAsms.Add(asmname, asm);
            return asm;
        }

        public ExtensionResult ExecMethod(Assembly asm, string typename, ExtensionContext context)
        {
            var command = asm.CreateInstance(typename) as IExtension;
            return command is null ? null : command.Execute(context);
            /*var t = asm.GetType(typename, true, true);
            var method = t.GetMethod(methodname);
            return method.Invoke(Activator.CreateInstance(t), new object[] { context });*/
        }

        public ExtensionResult LoadAndInvoke(string asmname, string typename, ExtensionContext context)
        {
            var asm = Load(asmname);
            return ExecMethod(asm, typename, context);
        }

        public void Dispose()
        {
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
