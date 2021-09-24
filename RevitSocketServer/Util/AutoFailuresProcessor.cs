using Autodesk.Revit.DB;
using ExtensionLib.Util;

namespace RevitSocketServer.Util
{
    public class AutoFailuresProcessor : IFailuresProcessor
    {
        public void Dismiss(Document document) { }

        public FailureProcessingResult ProcessFailures(FailuresAccessor failuresAccessor)
        {
            failuresAccessor.DeleteAllWarnings();
            foreach (var fail in failuresAccessor.GetFailureMessages())
            {
                Logger.WriteInfo($"Ситуация: {fail.GetDescriptionText()} - {fail.GetCurrentResolutionType()}");
            }
            return FailureProcessingResult.Continue;
        }
    }
}
