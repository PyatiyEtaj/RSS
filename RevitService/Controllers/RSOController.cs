using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RevitService.Models;
using RevitService.Providers;
using RevitService.Services.GoogleServices;
using RevitService.Services.NeedToLoadLinks;
using RevitService.Services.RSO.Logic;
using RevitService.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RSOController : DownloadController
    {
        private readonly ILogger<RSOController> _logger;
        private readonly IRSOService _rsoService;
        private readonly IRandomNameProvider _randomNameProvider;
        private readonly IClaimUserProvider _claimUserProvider;
        private readonly INeedToLoadLinks _needToLoadLinks;
        private readonly IGoogleRevitDocumentsProvider _googleRevitDocumentsProvider;

        public RSOController(
            ILogger<RSOController> logger,
            IRSOService rsoService,
            IRandomNameProvider randomNameProvider,
            IClaimUserProvider claimUserProvider,
            INeedToLoadLinks needToLoadLinks,
            IGoogleRevitDocumentsProvider googleRevitDocumentsProvider
        )
        {
            _logger = logger;
            _rsoService = rsoService;
            _randomNameProvider = randomNameProvider;
            _claimUserProvider = claimUserProvider;
            _needToLoadLinks = needToLoadLinks;
            _googleRevitDocumentsProvider = googleRevitDocumentsProvider;
        }

        /// <summary>
        /// Создать новый РСО запрос из bim360 документа
        /// </summary>
        /// <param name="authorization">Токен</param>
        /// <param name="downloadurn">urn для скачивания документов с BIM360</param>
        /// <param name="requestname">Имя запроса</param>
        /// <response code="202">Запрос успешно создан</response>
        /// <response code="400">Что-то пошло не так во время создания запроса к RSS</response>  
        /// <response code="401">Неавторизованный запрос</response>  
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> Create([FromHeader] string authorization, [FromQuery] string downloadurn, [FromQuery] string requestname)
        {
            var user = _claimUserProvider.From(HttpContext.User);
            (var name, var fullpath) = _randomNameProvider.Next(FileExtensions.OldExcel);
            try
            {
                _rsoService.Create(
                    authorization, 
                    user.UserId, 
                    requestname, 
                    downloadurn, 
                    fullpath,
                    await _needToLoadLinks.LoadOrNotAsync(requestname)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Исключение: {ex}");
                return BadRequest("Ошибка во время создания запроса к RSS");
            }
            return Accepted(name);
        }

        /// <summary>
        /// Создать новый РСО запрос используя файл с локального сервера.
        /// </summary>
        /// <param name="serverpath">Расположение файла на сервере</param>
        /// <param name="requestname">Имя запроса</param>
        /// <response code="202">Запрос успешно создан</response>
        /// <response code="400">Что-то пошло не так во время создания запроса к RSS</response>  
        /// <response code="401">Неавторизованный запрос</response> 
        [HttpPost]
        [Authorize] 
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateFromServerPath([FromQuery] string serverpath, [FromQuery] string requestname)
        {
            var user = _claimUserProvider.From(HttpContext.User);
            (var name, var fullpath) = _randomNameProvider.Next(FileExtensions.OldExcel);
            try
            {
                _rsoService.CreateFromServerPath(
                    user.UserId, 
                    requestname, 
                    serverpath, 
                    fullpath,
                    await _needToLoadLinks.LoadOrNotAsync(serverpath)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Исключение: {ex}");
                return BadRequest("Ошибка во время создания запроса к RSS");
            }
            return Accepted(name);
        }

        /// <summary>
        /// Отмена запроса
        /// </summary>
        /// <param name="key">Id запроса</param>
        /// <response code="202">Запрос на отмена принят</response>
        /// <response code="401">Неавторизованный запрос</response> 
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Cancel([FromQuery] string key)
        {
            var user = _claimUserProvider.From(HttpContext.User);
            var isCanceled = await _rsoService.CancelAsync(user, key);
            return Accepted(isCanceled);
        }

        /// <summary>
        /// Получить список запросов
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <response code="200">Список запросов</response>
        /// <response code="401">Неавторизованный запрос</response> 
        [HttpGet]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RevitRequest>>> Get([FromQuery]int page)
        {
            return Ok(await _rsoService.GetAsync(page));
        }

        /// <summary>
        /// Получить список только моих запросов
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <response code="200">Список моих запросов</response>
        /// <response code="401">Неавторизованный запрос</response> 
        [HttpGet]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RevitRequest>>> GetOnlyMy([FromQuery]int page)
        {
            var user = _claimUserProvider.From(HttpContext.User);
            return Ok(await _rsoService.GetByUserIdAsync(user.UserId, page));
        }

        /// <summary>
        /// Скачать эксель файл
        /// </summary>
        /// <param name="name">Имя файла на скачивание</param>
        /// <response code="206">Список моих запросов</response>
        /// <response code="400">Данный документ не существует</response> 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status206PartialContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Download([FromQuery] string name)
            => await DownloadByName(name);

        /// <summary>
        /// Получить список заготовленных revit файлов
        /// </summary>
        /// <response code="200">Список revit файлов</response> 
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GoogleRevitDocument>>> GetRevitDocuments()
            => Ok(await _googleRevitDocumentsProvider.GetAsync());
    }

}
