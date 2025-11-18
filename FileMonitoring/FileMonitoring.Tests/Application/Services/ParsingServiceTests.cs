using Xunit;
using FileMonitoring.Application.Services;
using System.Text;
using System;
using FileMonitoring.Domain.Enums;
using System.Globalization;

namespace FileMonitoring.Tests.Application.Services
{
    public class ParsingServiceTests
    {
        private readonly ParsingService _parsingService;

        public ParsingServiceTests()
        {
            _parsingService = new ParsingService();
        }

        private byte[] ToBytes(string s) => Encoding.UTF8.GetBytes(s);

        [Fact]
        public void ParsearArquivo_Tipo0_Valido_DeveParsearCorretamente()
        {
            // Arrange
            var linha = "012345678902023010120230101202301310000001UfCard  ";
            var conteudo = ToBytes(linha);

            // Act
            var resultado = _parsingService.ParsearArquivo(conteudo);

            // Assert
            Assert.Single(resultado);
            var transacao = resultado[0];
            Assert.Equal(TipoRegistro.Tipo0, transacao.TipoRegistro);
            Assert.Equal("1234567890", transacao.Estabelecimento);
            Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), transacao.DataProcessamento);
            Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), transacao.PeriodoInicial);
            Assert.Equal(new DateTime(2023, 1, 31, 0, 0, 0, DateTimeKind.Utc), transacao.PeriodoFinal);
            Assert.Equal("0000001", transacao.Sequencia);
            Assert.Equal("UfCard", transacao.Empresa);
        }

        [Fact]
        public void ParsearArquivo_Tipo1_Valido_DeveParsearCorretamente()
        {
            // Arrange
            var linha = "12023010112345678FagammonCard0000002";
            var conteudo = ToBytes(linha);

            // Act
            var resultado = _parsingService.ParsearArquivo(conteudo);

            // Assert
            Assert.Single(resultado);
            var transacao = resultado[0];
            Assert.Equal(TipoRegistro.Tipo1, transacao.TipoRegistro);
            Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), transacao.DataProcessamento);
            Assert.Equal("12345678", transacao.Estabelecimento);
            Assert.Equal("FagammonCard", transacao.Empresa);
            Assert.Equal("0000002", transacao.Sequencia);
            Assert.Equal(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), transacao.PeriodoInicial);
            Assert.Equal(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), transacao.PeriodoFinal);
        }

        [Fact]
        public void ParsearArquivo_ComConteudoVazio_DeveLancarExcecao()
        {
            // Arrange
            var conteudo = ToBytes("   ");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _parsingService.ParsearArquivo(conteudo));
            Assert.Equal("Arquivo está vazio ou contém apenas espaços em branco.", ex.Message);
        }

        [Fact]
        public void ParsearArquivo_ComLayoutInvalido_DeveLancarExcecao()
        {
            // Arrange
            var conteudo = ToBytes("2_registro_invalido");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _parsingService.ParsearArquivo(conteudo));
            Assert.Equal("Layout inválido: Tipo de registro '2' é desconhecido. Os tipos válidos são '0' e '1'.", ex.Message);
        }
        
        [Fact]
        public void ParsearArquivo_Tipo0_ComPadding_DeveParsearCorretamente()
        {
            // Arrange
            var linha = "012345678902023010120230101202301310000001UfCard"; // Sem padding
            var conteudo = ToBytes(linha);

            // Act
            var resultado = _parsingService.ParsearArquivo(conteudo);

            // Assert
            Assert.Single(resultado);
            Assert.Equal("UfCard", resultado[0].Empresa);
        }

        [Theory]
        [InlineData("012345678902023010120230101202301310000001UfCard  ", true, "")] // Tipo 0 Válido
        [InlineData("12023010112345678FagammonCard0000002", true, "")] // Tipo 1 Válido
        [InlineData("", false, "Layout inválido: a linha do arquivo está vazia.")]
        [InlineData("2", false, "Layout inválido: Tipo de registro '2' é desconhecido. Os tipos válidos são '0' e '1'.")]
        [InlineData("0_tamanho_errado", false, "Layout inválido: tipo '0' (UfCard) deve ter 50 caracteres, mas foram encontrados 16.")]
        [InlineData("1_tamanho_errado", false, "Layout inválido: tipo '1' (FagammonCard) deve ter 36 caracteres, mas foram encontrados 16.")]
        [InlineData("0ABCDEFGHIJ2023010120230101202301310000001UfCard  ", false, "Layout inválido (UfCard): O campo 'Estabelecimento' deve ser numérico.")]
        [InlineData("120230101ABCDEFGHFagammonCard0000002", false, "Layout inválido (FagammonCard): O campo 'Estabelecimento' deve ser numérico.")]
        public void ValidarLayout_TestaVariosCenarios(string linha, bool esperado, string mensagemEsperada)
        {
            // Act
            var (isValid, errorMessage) = _parsingService.ValidarLayout(linha);

            // Assert
            Assert.Equal(esperado, isValid);
            if (!isValid)
            {
                Assert.Equal(mensagemEsperada, errorMessage);
            }
        }
    }
}
