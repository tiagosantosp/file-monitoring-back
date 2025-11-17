using FileMonitoring.Application.DTOs;
using FileMonitoring.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileMonitoring.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArquivosController : ControllerBase
    {
        private readonly IArquivoService _arquivoService;

        public ArquivosController(IArquivoService arquivoService)
        {
            _arquivoService = arquivoService;
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> UploadArquivo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "O arquivo é obrigatório.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var result = await _arquivoService.ProcessarArquivoAsync(file);

            if (!result.Sucesso)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ArquivoDto>))]
        public async Task<IActionResult> GetArquivos()
        {
            var arquivos = await _arquivoService.ListarArquivosAsync();
            return Ok(arquivos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ArquivoDetalhadoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArquivo(int id)
        {
            var arquivo = await _arquivoService.ObterPorIdAsync(id);

            if (arquivo == null)
            {
                return NotFound();
            }

            return Ok(arquivo);
        }

        [HttpGet("estatisticas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstatisticasDto))]
        public async Task<IActionResult> GetEstatisticas()
        {
            var estatisticas = await _arquivoService.ObterEstatisticasAsync();
            return Ok(estatisticas);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteArquivo(int id)
        {
            try
            {
                await _arquivoService.DeletarArquivoAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
