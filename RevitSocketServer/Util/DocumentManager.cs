using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using ExtensionLib.Util;
using SocketServerEntities.Exceptions;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RevitSocketServer.Util
{
    public class DocumentManager : IDocumentManager
    {
        private readonly Application _app;

        private readonly DocumentLoader _loader
            = new DocumentLoader();

        private readonly ConcurrentQueue<string> _queueOfRequests
            = new ConcurrentQueue<string>();

        private readonly ConcurrentQueue<Document> _queueToClose
            = new ConcurrentQueue<Document>();

        private readonly ConcurrentQueue<Document> _queueOfResults
            = new ConcurrentQueue<Document>();

        private Document _currentLoaded;
        private string _currentOpenedDocument;

        public DocumentManager(Application app)
        {
            _app = app;
        }

        private void AddRequestToLoad(string filepath)
            => _queueOfRequests.Enqueue(filepath);

        private void AddRequestToClose(/*Document doc*/)
        {
            if (_currentLoaded is null) return;

            _queueToClose.Enqueue(_currentLoaded);
        }


        private async Task<Document> GetDocument()
        {
            while (_queueOfResults.Count == 0)
            {
                await Task.Delay(100);
            }
            return _queueOfResults.TryDequeue(out var doc) ? doc : default;
        }

        public async Task<Document> OpenDocumentAsync(string filepath, bool needToLoadLinks)
        {
            if (filepath == _currentOpenedDocument)
                return _currentLoaded;

            if (!(_currentLoaded is null))
            {
                Logger.WriteInfo($"Необходимо закрыть текущий документ, запрос создан.");
                AddRequestToClose();
            }
            AddRequestToLoad(filepath);
            return await GetDocument();
        }

        /// <summary>
        /// ВЫ ДОЛЖНЫ ИСПОЛЬЗОВАТЬ ДАННЫЙ МЕТОД ЛИШЬ В ГЛАВНОМ ПОТОКЕ РАСШИРЕНИЯ, ИНАЧЕ INNER REVIT EXCEPTIONS
        /// </summary>
        public void TryLoadQueueedDocument()
        {
            if (_queueToClose.Count == 0 && _queueOfRequests.TryDequeue(out var filepath))
            {
                Logger.WriteInfo($"\tperform load of {filepath} in main thread");
                var doc = _loader.OpenDocument(_app, filepath);
                _currentLoaded = doc;
                _currentOpenedDocument = filepath;
                _queueOfResults.Enqueue(doc);
                Logger.WriteInfo($"Успешное завершение");
            }
        }

        /// <summary>
        /// ВЫ ДОЛЖНЫ ИСПОЛЬЗОВАТЬ ДАННЫЙ МЕТОД ЛИШЬ В ГЛАВНОМ ПОТОКЕ РАСШИРЕНИЯ, ИНАЧЕ INNER REVIT EXCEPTIONS
        /// </summary>
        public void TryCloseQueueedDocument()
        {
            if (_queueToClose.TryDequeue(out var doc))
            {
                CloseDocumentAsync(doc);
            }
        }

        public Task<bool> CloseDocumentAsync(Document document)
        {
            var result = document.Close(false);
            if (result)
            {
                _currentLoaded = null;
                Logger.WriteInfo($"Успешное завершение");
                return Task.FromResult(result);
            }
            throw new DocumentManagerException("Не удалось закрыть документ");
        }
    }
}
