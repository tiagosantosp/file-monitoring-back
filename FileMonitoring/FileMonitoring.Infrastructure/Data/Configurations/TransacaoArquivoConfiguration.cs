using FileMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileMonitoring.Infrastructure.Data.Configurations;

public class TransacaoArquivoConfiguration : IEntityTypeConfiguration<TransacaoArquivo>
{
    public void Configure(EntityTypeBuilder<TransacaoArquivo> builder)
    {
        builder.ToTable("TransacoesArquivo");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.ArquivoId)
            .IsRequired();

        builder.Property(t => t.TipoRegistro)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Estabelecimento)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.DataProcessamento)
            .IsRequired();

        builder.Property(t => t.PeriodoInicial)
            .IsRequired();

        builder.Property(t => t.PeriodoFinal)
            .IsRequired();

        builder.Property(t => t.Sequencia)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(t => t.Empresa)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(t => t.ArquivoId);
        builder.HasIndex(t => t.Estabelecimento);
    }
}