using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ExtensionLib.Util
{
    public static class Logger
    {
        private enum MsgType
        {
            Info,
            Error
        }

        private static StreamWriter _textlog;
        private static bool _isavailable = false;

        public static void Init(string logname)
        {
            var logpath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Autodesk", "Revit", "Addins", "2019", "RevitSocketServer", $"{logname.Replace(' ', '_')}.log"
            );
            _textlog = new StreamWriter(logpath, true, Encoding.UTF8);
            _textlog.AutoFlush = true;
            _isavailable = true;
        }

        private static string Format(MsgType t, string caller, string msg) => $"{DateTime.Now} [{t}] [{caller}] {msg}";

        public static void WriteInfo(string str)
        {
            if (!_isavailable) return;
            StackFrame s = new StackFrame(1);
            var method = s.GetMethod();
            var msg = Format(MsgType.Info, $"{method.ReflectedType.Name}.{method.Name}", str);
            Console.WriteLine(msg);
            _textlog.Write($"{msg}\n");
        }

        public static void WriteError(string str)
        {
            if (!_isavailable) return;
            StackFrame s = new StackFrame(1);
            var method = s.GetMethod();
            var msg = Format(MsgType.Error, $"{method.ReflectedType.Name}.{method.Name}", str);
            Console.WriteLine(msg);
            _textlog.Write($"{msg}\n");
        }

        public static void Dispose()
        {
            _textlog.Dispose();
            _isavailable = false;
        }
    }

}
