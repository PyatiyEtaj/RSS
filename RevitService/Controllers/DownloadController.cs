using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using RevitService.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class DownloadController : ControllerBase
    {
        private List<string> _availableFilesTypes = new();
        protected DownloadController()
        {
            _availableFilesTypes.Add("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"); // xlsx
            _availableFilesTypes.Add("application/vnd.ms-excel"); // xls
        }

        protected void AddAvailbleTypeForUpload(string type)
        {
            _availableFilesTypes.Add(type);
        }

        protected async Task<MemoryStream> FileToStream(string filePath)
        {
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return memory;
        }

        protected string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        protected async Task<IActionResult> DownloadByName(string name)
        {
            string filePath = FileManager.GetFilePath(name);
            if (FileManager.IsNameSafe(filePath))
            {
                if (System.IO.File.Exists(filePath))
                {
                    var res = File(await FileToStream(filePath), GetContentType(filePath), name);
                    //System.IO.File.Delete(filePath);
                    return res;
                }
            }

            return BadRequest("Документ не найден");
        }

        protected async Task<bool> UploadFile(IFormFile file, string name)
        {
            if (_availableFilesTypes.FirstOrDefault(x => x == file.ContentType) != default)
            {
                var path = FileManager.GetFilePath(name);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return true;
            }
            return false;
        }
    }
}
