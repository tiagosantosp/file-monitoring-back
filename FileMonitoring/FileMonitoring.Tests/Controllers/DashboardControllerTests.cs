using Xunit;
using Moq;
using FileMonitoring.API.Controllers;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using System;

namespace FileMonitoring.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<IArquivoService> _mockArquivoService;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly DashboardController _controller;
        private const string ESTATISTICAS_CACHE_KEY = "dashboard_estatisticas";

        public DashboardControllerTests()
        {
            _mockArquivoService = new Mock<IArquivoService>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _controller = new DashboardController(_mockArquivoService.Object, _mockMemoryCache.Object);
        }

        [Fact]
        public async Task GetEstatisticas_QuandoCacheEstaVazio_DeveBuscarDoServicoESalvarNoCache()
        {
            // Arrange
            object? cachedValue = null;
            _mockMemoryCache.Setup(m => m.TryGetValue(ESTATISTICAS_CACHE_KEY, out cachedValue)).Returns(false);

            var estatisticasDto = new EstatisticasDto { TotalArquivos = 100 };
            _mockArquivoService.Setup(s => s.ObterEstatisticasAsync()).ReturnsAsync(estatisticasDto);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            // Act
            var result = await _controller.GetEstatisticas();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EstatisticasDto>(okResult.Value);
            Assert.Equal(100, returnValue.TotalArquivos);

            _mockArquivoService.Verify(s => s.ObterEstatisticasAsync(), Times.Once);
            _mockMemoryCache.Verify(m => m.CreateEntry(ESTATISTICAS_CACHE_KEY), Times.Once);
        }

        [Fact]
        public async Task GetEstatisticas_QuandoCacheExiste_DeveRetornarDoCache()
        {
            // Arrange
            object? estatisticasDto = new EstatisticasDto { TotalArquivos = 150 };
            _mockMemoryCache.Setup(m => m.TryGetValue(ESTATISTICAS_CACHE_KEY, out estatisticasDto)).Returns(true);

            // Act
            var result = await _controller.GetEstatisticas();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EstatisticasDto>(okResult.Value);
            Assert.Equal(150, returnValue.TotalArquivos);

            _mockArquivoService.Verify(s => s.ObterEstatisticasAsync(), Times.Never);
        }

        [Theory]
        [InlineData(90, "Saudável")]
        [InlineData(80, "Saudável")]
        [InlineData(79, "Atenção")]
        [InlineData(50, "Atenção")]
        [InlineData(49, "Crítico")]
        public async Task GetResumo_DeveRetornarStatusCorretoBaseadoNoPercentual(double percentual, string statusEsperado)
        {
            // Arrange
            var estatisticas = new EstatisticasDto { PercentualSucesso = percentual };
            _mockArquivoService.Setup(s => s.ObterEstatisticasAsync()).ReturnsAsync(estatisticas);

            // Act
            var result = await _controller.GetResumo();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic resumo = okResult.Value!;
            Assert.Equal(statusEsperado, resumo.GetType().GetProperty("status").GetValue(resumo, null));
        }

        [Fact]
        public void LimparCache_DeveChamarRemoveDoCache()
        {
            // Act
            var result = _controller.LimparCache();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockMemoryCache.Verify(m => m.Remove(ESTATISTICAS_CACHE_KEY), Times.Once);
        }
    }
}
