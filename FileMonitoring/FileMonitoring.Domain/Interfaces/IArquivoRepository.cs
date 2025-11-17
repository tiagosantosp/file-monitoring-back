using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;

namespace FileMonitoring.Domain.Interfaces;

public interface IArquivoRepository : IBaseRepository<Arquivo>
{
    Task<bool> ExistePorHashAsync(string hashMD5);
    Task<Arquivo?> GetByHashAsync(string hashMD5);
    Task<IEnumerable<Arquivo>> GetByStatusAsync(StatusArquivo status);
    Task<IEnumerable<Arquivo>> GetExpiradosAsync(DateTime dataReferencia);
    Task<Arquivo?> GetByIdComTransacoesAsync(int id);
    Task<Dictionary<StatusArquivo, int>> GetEstatisticasPorStatusAsync();
    Task<IEnumerable<Arquivo>> GetAllOrderedAsync();
}