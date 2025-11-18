using Xunit;
using Moq;
using FileMonitoring.API.Controllers;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace FileMonitoring.Tests.Controllers
{
    public class ArquivosControllerTests
    {
        private readonly Mock<IArquivoService> _mockArquivoService;
        private readonly ArquivosController _controller;

        public ArquivosControllerTests()
        {
            _mockArquivoService = new Mock<IArquivoService>();
            _controller = new ArquivosController(_mockArquivoService.Object);
        }

        [Fact]
        public async Task UploadArquivo_ComArquivoValido_DeveRetornarOk()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.Length).Returns(ms.Length);

            var uploadResult = new UploadResultDto { Sucesso = true, Mensagem = "Arquivo processado." };
            _mockArquivoService.Setup(s => s.ProcessarArquivoAsync(mockFile.Object)).ReturnsAsync(uploadResult);

            // Act
            var result = await _controller.UploadArquivo(mockFile.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UploadResultDto>(okResult.Value);
            Assert.True(returnValue.Sucesso);
        }

        [Fact]
        public async Task UploadArquivo_ComArquivoNulo_DeveRetornarBadRequest()
        {
            // Act
            var result = await _controller.UploadArquivo(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        }

        [Fact]
        public async Task UploadArquivo_QuandoServicoFalha_DeveRetornarBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1); // Valid length
            var uploadResult = new UploadResultDto { Sucesso = false, Mensagem = "Erro no processamento." };
            _mockArquivoService.Setup(s => s.ProcessarArquivoAsync(mockFile.Object)).ReturnsAsync(uploadResult);

            // Act
            var result = await _controller.UploadArquivo(mockFile.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<UploadResultDto>(badRequestResult.Value);
            Assert.False(returnValue.Sucesso);
        }

        [Fact]
        public async Task GetArquivos_DeveRetornarOkComListaDeArquivos()
        {
            // Arrange
            var arquivosDto = new List<ArquivoDto>
            {
                new ArquivoDto { Id = 1, NomeArquivo = "test1.txt", DataRecebimento = DateTime.UtcNow.AddDays(-1), Status = "Processado" },
                new ArquivoDto { Id = 2, NomeArquivo = "test2.txt", DataRecebimento = DateTime.UtcNow.AddDays(-2), Status = "Pendente" }
            };
            _mockArquivoService.Setup(service => service.ListarArquivosAsync()).ReturnsAsync(arquivosDto);

            // Act
            var result = await _controller.GetArquivos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ArquivoDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
            _mockArquivoService.Verify(service => service.ListarArquivosAsync(), Times.Once);
        }

        [Fact]
        public async Task GetArquivo_ComIdValido_DeveRetornarOkComArquivo()
        {
            // Arrange
            var arquivoId = 1;
            var arquivoDetalhadoDto = new ArquivoDetalhadoDto { Id = arquivoId, NomeArquivo = "details.txt" };
            _mockArquivoService.Setup(s => s.ObterPorIdAsync(arquivoId)).ReturnsAsync(arquivoDetalhadoDto);

            // Act
            var result = await _controller.GetArquivo(arquivoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ArquivoDetalhadoDto>(okResult.Value);
            Assert.Equal(arquivoId, returnValue.Id);
        }

        [Fact]
        public async Task GetArquivo_ComIdInvalido_DeveRetornarNotFound()
        {
            // Arrange
            var arquivoId = 99;
            _mockArquivoService.Setup(s => s.ObterPorIdAsync(arquivoId)).ReturnsAsync((ArquivoDetalhadoDto)null);

            // Act
            var result = await _controller.GetArquivo(arquivoId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteArquivo_ComIdValido_DeveRetornarNoContent()
        {
            // Arrange
            var arquivoId = 1;
            _mockArquivoService.Setup(s => s.DeletarArquivoAsync(arquivoId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteArquivo(arquivoId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockArquivoService.Verify(s => s.DeletarArquivoAsync(arquivoId), Times.Once);
        }

        [Fact]
        public async Task DeleteArquivo_ComIdInvalido_DeveRetornarNotFound()
        {
            // Arrange
            var arquivoId = 99;
            _mockArquivoService.Setup(s => s.DeletarArquivoAsync(arquivoId)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.DeleteArquivo(arquivoId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteExpired_DeveRetornarOkComContagem()
        {
            // Arrange
            var count = 5;
            _mockArquivoService.Setup(s => s.DeletarExpiradosAsync()).ReturnsAsync(count);

            // Act
            var result = await _controller.DeleteExpired();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
            var propQuantidade = value.GetType().GetProperty("quantidade");
            Assert.NotNull(propQuantidade);
            var quantidade = propQuantidade.GetValue(value, null);
            Assert.Equal(count, quantidade);
        }
    }
}
