namespace FileMonitoring.Application.DTOs;

public class EstatisticasDto
{
    public int TotalArquivos { get; set; }
    public int ArquivosRecepcionados { get; set; }
    public int ArquivosNaoRecepcionados { get; set; }
    public double PercentualSucesso { get; set; }
    public Dictionary<string, int> PorAdquirente { get; set; } = new();
}