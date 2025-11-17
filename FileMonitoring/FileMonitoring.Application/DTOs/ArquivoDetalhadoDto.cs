namespace FileMonitoring.Application.DTOs;

public class ArquivoDetalhadoDto
{
    public int Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public DateTime DataRecebimento { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TipoAdquirente { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public string CaminhoBackup { get; set; } = string.Empty;
    public string HashMD5 { get; set; } = string.Empty;
    public string? MensagemErro { get; set; }
    public List<TransacaoArquivoDto> Transacoes { get; set; } = new();
}