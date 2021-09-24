using ExtensionLib.Entity;
using Microsoft.Extensions.Logging;
using RevitService.Models;
using RevitService.Services.GoogleServices;
using RevitService.Services.KS.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Logic
{
    public class KsFastService : AKsService
    {
        private readonly ILogger<KsFastService> _logger;
        private readonly IElementRepository _repository;
        private readonly KsCreator _ks = new();
        private readonly GoogleDriveService _service;
        public KsFastService(GoogleDriveService service, ILogger<KsFastService> logger, IElementRepository repository)
        {
            _logger = logger;
            _repository = repository;
            _service = service;
        }

        public override async Task<bool> CreateAsync(string filepath, string authorization, KsDTO dto, CancellationToken cancellationToken = default)
        {
            if (await GetGoogleDocAsync(_service))
            {
                _logger.LogInformation("Получаем данные из монги...");
                var res = await _repository.GetAsync(dto.urn, dto.start, dto.end);
                _logger.LogInformation("Создаем таблицу...");
                var isCreated = await _ks.CreateAsync(res, FilePathes.Template, filepath);
                _logger.LogInformation(isCreated ? "успешное завершение" : "что-то пошло не так");
                return isCreated;
            }

            return false;
        }

    }
}
