namespace FileMonitoring.Application.DTOs;

public class UploadResultDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public ArquivoDto? Arquivo { get; set; }
}