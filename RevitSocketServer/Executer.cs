using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using RevitSocketServer.Server;
using RevitSocketServer.Util;
using System.Threading;
using System.Threading.Tasks;

namespace RevitSocketServer
{
    public class Executer : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
            => Result.Succeeded;

        public Result OnStartup(UIControlledApplication a)
        {
            a.ControlledApplication.ApplicationInitialized
                += OnApplicationInitialized;
            return Result.Succeeded;
        }        

        void InitFailureProcessor()
        {
            IFailuresProcessor myFail = new AutoFailuresProcessor();
            Autodesk.Revit.ApplicationServices.Application.RegisterFailuresProcessor(myFail);
        }

        void OnApplicationInitialized(
            object sender,
            ApplicationInitializedEventArgs e)
        {
            Application app = sender as Application;
            WinApi.OpenConsole();
            InitFailureProcessor();
            RevitPatcher.ThirdPartyUpdaterDialog();

            string adr = "127.0.0.1";
            int port = 11000;
            IDocumentManager manager = new DocumentManagerImproved(app);
            Task.Run(async() =>
            {
                using (var server = new ServerObject(manager))
                {
                    await server.Listen(adr, port);
                }
            });
            /*while (true)
            {
                manager.TryCloseQueueedDocument();
                manager.TryLoadQueueedDocument();
                Thread.Sleep(200);
            }*/
        }
    }
}
