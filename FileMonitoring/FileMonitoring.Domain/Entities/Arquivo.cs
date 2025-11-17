using FileMonitoring.Domain.Enums;

namespace FileMonitoring.Domain.Entities;

public class Arquivo
{
    public int Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public DateTime DataRecebimento { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public StatusArquivo Status { get; set; }
    public string CaminhoBackup { get; set; } = string.Empty;
    public string HashMD5 { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public TipoAdquirente TipoAdquirente { get; set; }
    public string? MensagemErro { get; set; }

    public virtual ICollection<TransacaoArquivo> Transacoes { get; set; } = new List<TransacaoArquivo>();
}