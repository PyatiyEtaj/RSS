using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using ExtensionLib.Util;
using SocketServerEntities.Exceptions;
using System.Threading.Tasks;

namespace RevitSocketServer.Util
{
    public class DocumentManagerImproved : IDocumentManager
    {
        private readonly Application _app;

        private readonly DocumentLoader _loader
            = new DocumentLoader();

        private readonly TaskScheduler _contextSource
            = TaskScheduler.FromCurrentSynchronizationContext();

        private Document _currentLoaded;
        private string _currentOpenedDocument;

        public DocumentManagerImproved(Application app)
        {
            _app = app;
        }

        private Task<bool> Close(Document document)
        {
            return Task.Factory.StartNew(
                    () =>
                    {
                        return document.Close(false);
                    },
                    default,
                    TaskCreationOptions.AttachedToParent,
                    _contextSource
                );
        }

        public async Task<bool> CloseDocumentAsync(Document document)
        {
            var result = await Close(document);
            if (!result)
            {
                throw new DocumentManagerException("Не удалось закрыть документ");
            }

            _currentLoaded = default;
            _currentOpenedDocument = default;
            Logger.WriteInfo($"Успешное закрытие документа");
            return result;
        }

        private Task<Document> OpenNewDocumentAsync(string filepath, bool needToLoadLinks)
        {
            return Task.Factory.StartNew(
                    () =>
                    {
                        return needToLoadLinks 
                            ? _loader.OpenDocument(_app, filepath) 
                            : _loader.OpenDocumentWithWorksets(
                                _app, 
                                filepath, 
                                WorksetConfigurationOption.CloseAllWorksets
                              );
                    },
                    default,
                    TaskCreationOptions.AttachedToParent,
                    _contextSource
                );
        }

        public async Task<Document> OpenDocumentAsync(string filepath, bool needToLoadLinks)
        {
            Logger.WriteInfo($"filepath = {filepath} [needToLoadlinks = {needToLoadLinks}]");
            if (filepath == _currentOpenedDocument)
                return _currentLoaded;

            if (!(_currentLoaded is null))
            {
                Logger.WriteInfo($"Необходимо закрыть текущий документ.");
                await CloseDocumentAsync(_currentLoaded);
            }
            _currentLoaded = await OpenNewDocumentAsync(filepath, needToLoadLinks);
            _currentOpenedDocument = filepath;
            return _currentLoaded;
        }
    }
}
