using FileMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FileMonitoring.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Arquivo> Arquivos { get; set; }
    public DbSet<TransacaoArquivo> TransacoesArquivo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}