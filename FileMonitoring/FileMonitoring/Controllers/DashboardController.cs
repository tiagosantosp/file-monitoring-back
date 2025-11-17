using FileMonitoring.Application.DTOs;
using FileMonitoring.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FileMonitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IArquivoService _arquivoService;
    private readonly IMemoryCache _cache;
    private const string ESTATISTICAS_CACHE_KEY = "dashboard_estatisticas";
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

    public DashboardController(IArquivoService arquivoService, IMemoryCache cache)
    {
        _arquivoService = arquivoService;
        _cache = cache;
    }

    [HttpGet("estatisticas")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstatisticasDto))]
    public async Task<IActionResult> GetEstatisticas()
    {
        if (!_cache.TryGetValue(ESTATISTICAS_CACHE_KEY, out EstatisticasDto? estatisticas))
        {
            estatisticas = await _arquivoService.ObterEstatisticasAsync();

            _cache.Set(ESTATISTICAS_CACHE_KEY, estatisticas, CACHE_DURATION);
        }

        return Ok(estatisticas);
    }

    
    [HttpGet("resumo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumo()
    {
        var estatisticas = await _arquivoService.ObterEstatisticasAsync();

        var resumo = new
        {
            totalArquivos = estatisticas.TotalArquivos,
            recepcionados = estatisticas.ArquivosRecepcionados,
            naoRecepcionados = estatisticas.ArquivosNaoRecepcionados,
            percentualSucesso = estatisticas.PercentualSucesso,
            status = estatisticas.PercentualSucesso >= 80 ? "Saudável" :
                     estatisticas.PercentualSucesso >= 50 ? "Atenção" : "Crítico"
        };

        return Ok(resumo);
    }

    [HttpPost("limpar-cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult LimparCache()
    {
        _cache.Remove(ESTATISTICAS_CACHE_KEY);
        return Ok(new { mensagem = "Cache limpo com sucesso." });
    }
}