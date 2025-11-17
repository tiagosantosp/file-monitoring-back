using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;
using FileMonitoring.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileMonitoring.Infrastructure.Data.Repositories;

public class ArquivoRepository : BaseRepository<Arquivo>, IArquivoRepository
{
    public ArquivoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistePorHashAsync(string hashMD5)
    {
        return await _dbSet.AnyAsync(a => a.HashMD5 == hashMD5);
    }

    public async Task<Arquivo?> GetByHashAsync(string hashMD5)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.HashMD5 == hashMD5);
    }

    public async Task<IEnumerable<Arquivo>> GetByStatusAsync(StatusArquivo status)
    {
        return await _dbSet
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.DataRecebimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<Arquivo>> GetExpiradosAsync(DateTime dataReferencia)
    {
        return await _dbSet
            .Where(a => a.DataExpiracao.HasValue && a.DataExpiracao.Value <= dataReferencia)
            .ToListAsync();
    }

    public async Task<Arquivo?> GetByIdComTransacoesAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Transacoes)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Dictionary<StatusArquivo, int>> GetEstatisticasPorStatusAsync()
    {
        return await _dbSet
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task<IEnumerable<Arquivo>> GetAllOrderedAsync()
    {
        return await _dbSet
            .OrderByDescending(a => a.DataRecebimento)
            .ToListAsync();
    }
}