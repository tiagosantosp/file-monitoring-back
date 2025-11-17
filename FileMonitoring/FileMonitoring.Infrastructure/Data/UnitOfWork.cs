using FileMonitoring.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileMonitoring.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IArquivoRepository Arquivos { get; }
    public ITransacaoArquivoRepository Transacoes { get; }

    public UnitOfWork(
        AppDbContext context,
        IArquivoRepository arquivoRepository,
        ITransacaoArquivoRepository transacaoRepository)
    {
        _context = context;
        Arquivos = arquivoRepository;
        Transacoes = transacaoRepository;
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}