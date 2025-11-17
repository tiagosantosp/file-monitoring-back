using FileMonitoring.Domain.Entities;

namespace FileMonitoring.Domain.Interfaces;

public interface ITransacaoArquivoRepository : IBaseRepository<TransacaoArquivo>
{
    Task<IEnumerable<TransacaoArquivo>> GetByArquivoIdAsync(int arquivoId);
    Task<IEnumerable<TransacaoArquivo>> GetByEstabelecimentoAsync(string estabelecimento);
}