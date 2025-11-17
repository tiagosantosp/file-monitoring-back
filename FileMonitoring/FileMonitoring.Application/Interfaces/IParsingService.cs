using FileMonitoring.Domain.Entities;

namespace FileMonitoring.Application.Interfaces;

public interface IParsingService
{
    List<TransacaoArquivo> ParsearArquivo(byte[] conteudo);
}