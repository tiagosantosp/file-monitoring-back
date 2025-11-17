using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileMonitoring.Infrastructure.Data.Repositories;

public class TransacaoArquivoRepository : BaseRepository<TransacaoArquivo>, ITransacaoArquivoRepository
{
    public TransacaoArquivoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransacaoArquivo>> GetByArquivoIdAsync(int arquivoId)
    {
        return await _dbSet
            .Where(t => t.ArquivoId == arquivoId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransacaoArquivo>> GetByEstabelecimentoAsync(string estabelecimento)
    {
        return await _dbSet
            .Where(t => t.Estabelecimento == estabelecimento)
            .Include(t => t.Arquivo)
            .OrderByDescending(t => t.DataProcessamento)
            .ToListAsync();
    }
}