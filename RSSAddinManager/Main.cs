using ExtensionLib.Extension;
using ExtensionLib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RSSAddinManager
{
    public class Main
    {
        public static readonly string AssemblyPossibleDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk", "Revit", "Addins", "2019", "RevitSocketServer");

        private Assembly Load(string asmname)
            => Assembly.LoadFrom(Path.Combine(AssemblyPossibleDir, "extensions", asmname));

        private ExtensionResult ExecMethod(string filePath, string typename, ExtensionContext context)
        {
			AssemLoader assemLoader = new AssemLoader();
			try
			{
				assemLoader.HookAssemblyResolve();
				Assembly assembly = assemLoader.LoadAddinsToTempFolder(filePath, false);
				if (assembly != null)
				{
					var command = assembly.CreateInstance(typename) as IExtension;
					return command is null ? null : command.Execute(context);
				}
			}
			/*catch (Exception ex)
			{
				Logger.WriteError(ex.ToString());
				throw new Exception(ex.Message);
			}*/
			finally
			{
				assemLoader.UnhookAssemblyResolve();
				assemLoader.CopyGeneratedFilesBack();
			}
			return null;
		}

        public ExtensionResult LoadAndInvoke(string asmname, string typename, ExtensionContext context)
		{
			var filePath = Path.Combine(AssemblyPossibleDir, "extensions", asmname);
			return ExecMethod(filePath, typename, context);
        }
    }
}
