using FileMonitoring.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace FileMonitoring.Application.Interfaces;

public interface IArquivoService
{
    Task<UploadResultDto> ProcessarArquivoAsync(IFormFile file);
    Task<IEnumerable<ArquivoDto>> ListarArquivosAsync();
    Task<ArquivoDetalhadoDto?> ObterPorIdAsync(int id);
    Task<EstatisticasDto> ObterEstatisticasAsync();
    Task<int> DeletarExpiradosAsync();
    Task DeletarArquivoAsync(int id);
}