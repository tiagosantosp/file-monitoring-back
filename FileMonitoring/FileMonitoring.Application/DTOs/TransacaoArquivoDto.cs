namespace FileMonitoring.Application.DTOs;

public class TransacaoArquivoDto
{
    public int Id { get; set; }
    public string TipoRegistro { get; set; } = string.Empty;
    public string Estabelecimento { get; set; } = string.Empty;
    public DateTime DataProcessamento { get; set; }
    public DateTime PeriodoInicial { get; set; }
    public DateTime PeriodoFinal { get; set; }
    public string Sequencia { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;
}