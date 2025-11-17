using FileMonitoring.Domain.Enums;

namespace FileMonitoring.Domain.Entities;

public class TransacaoArquivo
{
    public int Id { get; set; }
    public int ArquivoId { get; set; }
    public TipoRegistro TipoRegistro { get; set; }
    public string Estabelecimento { get; set; } = string.Empty;
    public DateTime DataProcessamento { get; set; }
    public DateTime PeriodoInicial { get; set; }
    public DateTime PeriodoFinal { get; set; }
    public string Sequencia { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;

    public virtual Arquivo Arquivo { get; set; } = null!;
}