using FileMonitoring.Application.Interfaces;
using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;
using System.Globalization;
using System.Text;

namespace FileMonitoring.Application.Services;

public class ParsingService : IParsingService
{
    public List<TransacaoArquivo> ParsearArquivo(byte[] conteudo)
    {
        var transacoes = new List<TransacaoArquivo>();
        var conteudoTexto = Encoding.UTF8.GetString(conteudo).Trim();

        if (string.IsNullOrWhiteSpace(conteudoTexto))
        {
            throw new InvalidOperationException("Arquivo está vazio");
        }

        var linha = conteudoTexto.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");

        if (!ValidarLayout(linha))
        {
            throw new InvalidOperationException("Layout do arquivo é inválido");
        }

        var tipoRegistro = linha[0];

        if (tipoRegistro == '0')
        {
            transacoes.Add(ParsearTipo0(linha));
        }
        else if (tipoRegistro == '1')
        {
            transacoes.Add(ParsearTipo1(linha));
        }
        else
        {
            throw new InvalidOperationException($"Tipo de registro inválido: {tipoRegistro}");
        }

        return transacoes;
    }

    public bool ValidarLayout(string linha)
    {
        if (string.IsNullOrWhiteSpace(linha))
            return false;

        var tipoRegistro = linha[0];

        if (tipoRegistro == '0' && linha.Length == 50)
            return true;

        if (tipoRegistro == '1' && linha.Length == 36)
            return true;

        return false;
    }

    private TransacaoArquivo ParsearTipo0(string linha)
    {
        if (linha.Length != 50)
        {
            throw new InvalidOperationException($"Layout tipo 0 deve ter 50 caracteres. Recebido: {linha.Length}");
        }

        return new TransacaoArquivo
        {
            TipoRegistro = TipoRegistro.Tipo0,
            Estabelecimento = linha.Substring(1, 10).Trim(),
            DataProcessamento = ParsearData(linha.Substring(11, 8)),
            PeriodoInicial = ParsearData(linha.Substring(19, 8)),
            PeriodoFinal = ParsearData(linha.Substring(27, 8)),
            Sequencia = linha.Substring(35, 7).Trim(),
            Empresa = linha.Substring(42, 8).Trim()
        };
    }

    private TransacaoArquivo ParsearTipo1(string linha)
    {
        if (linha.Length != 36)
        {
            throw new InvalidOperationException($"Layout tipo 1 deve ter 36 caracteres. Recebido: {linha.Length}");
        }

        return new TransacaoArquivo
        {
            TipoRegistro = TipoRegistro.Tipo1,
            DataProcessamento = ParsearData(linha.Substring(1, 8)),
            Estabelecimento = linha.Substring(9, 8).Trim(),
            Empresa = linha.Substring(17, 12).Trim(),
            Sequencia = linha.Substring(29, 7).Trim(),
            PeriodoInicial = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
            PeriodoFinal = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
        };
    }

    private DateTime ParsearData(string data)
    {
        if (string.IsNullOrWhiteSpace(data) || data.Length != 8)
        {
            throw new InvalidOperationException($"Data inválida: {data}");
        }

        if (!DateTime.TryParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var resultado))
        {
            throw new InvalidOperationException($"Formato de data inválido: {data}");
        }

        return resultado;
    }
}