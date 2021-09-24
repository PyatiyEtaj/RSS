using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Threading;

namespace ExtensionLib.Extension
{
    public class ExtensionContext
    {
        public Dictionary<string, object> Args { get; set; } = new Dictionary<string, object>();
        public Document Document { get; set; }
        public CancellationToken cancellationToken { get; set; }
    }
}
