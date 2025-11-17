using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileMonitoring.Infrastructure.Data.Configurations;

public class ArquivoConfiguration : IEntityTypeConfiguration<Arquivo>
{
    public void Configure(EntityTypeBuilder<Arquivo> builder)
    {
        builder.ToTable("Arquivos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.NomeArquivo)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.DataRecebimento)
            .IsRequired();

        builder.Property(a => a.DataExpiracao)
            .IsRequired(false);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.CaminhoBackup)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.HashMD5)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(a => a.HashMD5)
            .IsUnique();

        builder.Property(a => a.TamanhoBytes)
            .IsRequired();

        builder.Property(a => a.TipoAdquirente)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.MensagemErro)
            .HasMaxLength(1000);

        builder.HasMany(a => a.Transacoes)
            .WithOne(t => t.Arquivo)
            .HasForeignKey(t => t.ArquivoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}