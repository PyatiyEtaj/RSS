using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using ExtensionLib.Util;
using SocketServerEntities.Exceptions;
using System;
using System.Collections.Generic;

namespace RevitSocketServer.Util
{
    public class DocumentLoader
    {
        public Document OpenDocument(Application app, string file)
        {
            Document doc = null;
            try
            {
                doc = app.OpenDocumentFile(file);
            }
            catch (Exception e)
            {
                throw new DocumentLoaderException($"Ошибка во время открытия файла {e.Message}");
            }

            return doc;
        }

        public Document OpenDocumentWithWorksets(
            Application app,
            string file,
            WorksetConfigurationOption options = WorksetConfigurationOption.CloseAllWorksets
        )
        {
            Document doc = null;
            var path = new FilePath(file);
            try
            {
                IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(path);
                IList<WorksetId> worksetIds = new List<WorksetId>();
                foreach (WorksetPreview worksetPrev in worksets)
                {
                    worksetIds.Add(worksetPrev.Id);
                }

                var openOptions = new OpenOptions();
                openOptions.AllowOpeningLocalByWrongUser = true;
                var openConfig = new WorksetConfiguration(options);
                /*if (options == WorksetConfigurationOption.CloseAllWorksets)
                    openConfig.Close(worksetIds);
                else 
                    openConfig.Open(worksetIds);*/
                openOptions.SetOpenWorksetsConfiguration(openConfig);

                doc = app.OpenDocumentFile(path, openOptions);
            }
            catch (Exception e)
            {
                throw new DocumentLoaderException($"Ошибка во время открытия файла {e.Message}");
            }

            return doc;
        }
    }
}
