namespace FileMonitoring.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IArquivoRepository Arquivos { get; }
    ITransacaoArquivoRepository Transacoes { get; }

    Task<int> CommitAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}