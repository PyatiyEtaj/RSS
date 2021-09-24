using RevitService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Threading;
using RevitService.Util;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using RevitService.Services.KS.Repository;
using RevitService.Services.KS.Logic;

namespace RevitService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]    
    public class KsController : DownloadController
    {
        private ILogger<KsController> _logger;
        private AKsService _ksService;
        private IKSRepository _ksRepository;

        public KsController(ILogger<KsController> logger, AKsService ksService, IKSRepository ksRepository)
        {
            _logger = logger;
            _ksService = ksService;
            _ksRepository = ksRepository;
        }        

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Create([FromHeader] string authorization, KsDTO ksdto, CancellationToken cancellationToken)
        {
            string name = $"{Guid.NewGuid()}.xlsx";
            string filePath = FileManager.GetFilePath(name);
            var isCreated = false;
            try
            {
                isCreated = await _ksService.CreateAsync(filePath, authorization, ksdto, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
            }
            if (isCreated)
            {
                await _ksRepository.InsertDocumentAsync(new KsDocument
                {
                    Start = ksdto.start,
                    End = ksdto.end,
                    FilePath = name,
                    Roles = ksdto.roles,
                    Name = $"KC {ksdto.name}",
                    Urn = ksdto.urn,
                    HubId = ksdto.hubId,
                    ProjectId = ksdto.projectId,
                    FilePathChanged = default,
                    IsApproved = false
                });
            }
            return isCreated ? Ok(name) : BadRequest("Не удалось создать документ");
        }

        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> Download([FromQuery] string name)
            => await DownloadByName(name);

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Upload(IFormFile file, [FromHeader]string documentId)
        {
            var bimdocument = await _ksRepository.FindByIdAsync(new ObjectId(documentId));
            string name = bimdocument.FilePathChanged != default ? bimdocument.FilePathChanged : $"[upd]{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";            
            if ( await UploadFile(file, name))
            {
                if (bimdocument.FilePathChanged == default)
                    await _ksRepository.UpdateDocumentFilePathChangedAsync(bimdocument, name);
                return await new ValueTask<IActionResult>(Ok("File has uploaded"));
            }

            return await new ValueTask<IActionResult>(BadRequest("File ContentType not supported"));
        }
    }
}
