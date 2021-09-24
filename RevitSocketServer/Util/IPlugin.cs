using ExtensionLib.Extension;

namespace RevitSocketServer.Util
{
    internal interface IPlugin
    {
        ExtensionResult LoadAndInvoke(string asmname, string typename, ExtensionContext context);
        void Dispose();
    }
}
