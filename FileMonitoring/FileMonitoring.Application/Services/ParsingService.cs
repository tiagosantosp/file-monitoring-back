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
            throw new InvalidOperationException("Arquivo está vazio ou contém apenas espaços em branco.");
        }

        var linha = conteudoTexto.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");

        if (linha.Length > 0 && linha[0] == '0' && linha.Length < 50)
        {
            linha = linha.PadRight(50, ' '); 
        }

        var (isValid, errorMessage) = ValidarLayout(linha);
        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage);
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

        return transacoes;
    }

    public (bool IsValid, string ErrorMessage) ValidarLayout(string linha)
    {
        if (string.IsNullOrWhiteSpace(linha)) 
            return (false, "Layout inválido: a linha do arquivo está vazia.");

        var tipoRegistro = linha[0];

        if (tipoRegistro == '0')
        {
            if (linha.Length != 50) 
                return (false, $"Layout inválido: tipo '0' (UfCard) deve ter 50 caracteres, mas foram encontrados {linha.Length}.");
            
            if (!IsAllDigits(linha.Substring(1, 10))) return (false, "Layout inválido (UfCard): O campo 'Estabelecimento' deve ser numérico.");
            if (!IsAllDigits(linha.Substring(11, 8))) return (false, "Layout inválido (UfCard): O campo 'DataProcessamento' deve ser numérico e no formato AAAAMMDD.");
            if (!IsAllDigits(linha.Substring(19, 8))) return (false, "Layout inválido (UfCard): O campo 'PeriodoInicial' deve ser numérico e no formato AAAAMMDD.");
            if (!IsAllDigits(linha.Substring(27, 8))) return (false, "Layout inválido (UfCard): O campo 'PeriodoFinal' deve ser numérico e no formato AAAAMMDD.");
            if (!IsAllDigits(linha.Substring(35, 7))) return (false, "Layout inválido (UfCard): O campo 'Sequência' deve ser numérico.");
            if (!linha.Substring(42, 8).Trim().Equals("UfCard", StringComparison.OrdinalIgnoreCase)) 
                return (false, "Layout inválido (UfCard): O campo 'Empresa' deve ser 'UfCard'.");

            return (true, string.Empty);
        }
        
        if (tipoRegistro == '1')
        {
            if (linha.Length != 36) 
                return (false, $"Layout inválido: tipo '1' (FagammonCard) deve ter 36 caracteres, mas foram encontrados {linha.Length}.");

            if (!IsAllDigits(linha.Substring(1, 8))) return (false, "Layout inválido (FagammonCard): O campo 'DataProcessamento' deve ser numérico e no formato AAAAMMDD.");
            if (!IsAllDigits(linha.Substring(9, 8))) return (false, "Layout inválido (FagammonCard): O campo 'Estabelecimento' deve ser numérico.");
            if (!linha.Substring(17, 12).Trim().Equals("FagammonCard", StringComparison.OrdinalIgnoreCase))
                return (false, "Layout inválido (FagammonCard): O campo 'Empresa' deve ser 'FagammonCard'.");
            if (!IsAllDigits(linha.Substring(29, 7))) return (false, "Layout inválido (FagammonCard): O campo 'Sequência' deve ser numérico.");

            return (true, string.Empty);
        }

        return (false, $"Layout inválido: Tipo de registro '{tipoRegistro}' é desconhecido. Os tipos válidos são '0' e '1'.");
    }

    private TransacaoArquivo ParsearTipo0(string linha)
    {
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
        if (!DateTime.TryParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var resultado))
        {
            throw new InvalidOperationException($"Formato de data inválido: {data}");
        }

        return resultado;
    }

    private bool IsAllDigits(string s) => s.All(char.IsDigit);
}