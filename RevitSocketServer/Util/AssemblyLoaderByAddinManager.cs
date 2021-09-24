using ExtensionLib.Extension;
using System;
using System.IO;
using System.Reflection;

namespace RevitSocketServer.Util
{
    internal class AssemblyLoaderByAddinManager : IPlugin
    {
        private readonly Assembly _addinManager;
        public static readonly string AssemblyPossibleDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk", "Revit", "Addins", "2019", "RevitSocketServer");

        public AssemblyLoaderByAddinManager()
        {
            string p = Path.Combine(AssemblyPossibleDir, "files");
            string ext = Path.Combine(AssemblyPossibleDir, "extensions");
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            if (!Directory.Exists(ext))
                Directory.CreateDirectory(ext);
            _addinManager = Assembly.LoadFrom(Path.Combine(AssemblyPossibleDir, "RSSAddinManager.dll"));
        }


        public ExtensionResult LoadAndInvoke(string asmname, string typename, ExtensionContext context)
        {
            var t = _addinManager.GetType("RSSAddinManager.Main", true, true);
            var method = t.GetMethod("LoadAndInvoke");
            return (ExtensionResult)method.Invoke(Activator.CreateInstance(t), new object[] { asmname, typename, context });
        }

        public void Dispose()
        {
            for (int i = 0; i < 6; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
