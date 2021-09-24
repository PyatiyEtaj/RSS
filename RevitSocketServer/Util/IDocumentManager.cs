using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RevitSocketServer.Util
{
    public interface IDocumentManager
    {
        Task<Document> OpenDocumentAsync(string filepath, bool needToLoadLinks);
        Task<bool> CloseDocumentAsync(Document document);
    }
}
