using AutoMapper;
using FileMonitoring.Application.DTOs;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;
using FileMonitoring.Domain.Interfaces;
using FileMonitoring.Infrastructure.FileStorage;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace FileMonitoring.Application.Services;

public class ArquivoService : IArquivoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParsingService _parsingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public ArquivoService(
        IUnitOfWork unitOfWork,
        IParsingService parsingService,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _parsingService = parsingService;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<UploadResultDto> ProcessarArquivoAsync(IFormFile file)
    {
        try
        {
            byte[] conteudo;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                conteudo = ms.ToArray();
            }

            var hash = CalcularMD5(conteudo);

            if (await _unitOfWork.Arquivos.ExistePorHashAsync(hash))
            {
                return new UploadResultDto
                {
                    Sucesso = false,
                    Mensagem = "Arquivo duplicado. Este arquivo já foi processado anteriormente."
                };
            }

            var transacoes = _parsingService.ParsearArquivo(conteudo);
            var caminhoBackup = await _fileStorageService.SaveFileAsync(file.FileName, conteudo);

            var arquivo = new Arquivo
            {
                NomeArquivo = file.FileName,
                DataRecebimento = DateTime.UtcNow,
                DataExpiracao = DateTime.UtcNow.AddDays(30),
                Status = StatusArquivo.Recepcionado,
                CaminhoBackup = caminhoBackup,
                HashMD5 = hash,
                TamanhoBytes = conteudo.Length,
                TipoAdquirente = DeterminarTipoAdquirente(transacoes.First().Empresa),
                Transacoes = transacoes
            };

            await _unitOfWork.Arquivos.AddAsync(arquivo);
            await _unitOfWork.CommitAsync();

            return new UploadResultDto
            {
                Sucesso = true,
                Mensagem = "Arquivo processado com sucesso",
                Arquivo = _mapper.Map<ArquivoDto>(arquivo)
            };
        }
        catch (Exception ex)
        {
            try
            {
                byte[] conteudo;
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    conteudo = ms.ToArray();
                }

                var hash = CalcularMD5(conteudo);
                var caminhoBackup = await _fileStorageService.SaveFileAsync(file.FileName, conteudo);

                var arquivo = new Arquivo
                {
                    NomeArquivo = file.FileName,
                    DataRecebimento = DateTime.UtcNow,
                    DataExpiracao = DateTime.UtcNow.AddDays(7),
                    Status = StatusArquivo.NaoRecepcionado,
                    CaminhoBackup = caminhoBackup,
                    HashMD5 = hash,
                    TamanhoBytes = conteudo.Length,
                    TipoAdquirente = TipoAdquirente.UfCard,
                    MensagemErro = ex.Message
                };

                await _unitOfWork.Arquivos.AddAsync(arquivo);
                await _unitOfWork.CommitAsync();
            }
            catch { }

            return new UploadResultDto
            {
                Sucesso = false,
                Mensagem = $"Erro ao processar arquivo: {ex.Message}"
            };
        }
    }

    public async Task<IEnumerable<ArquivoDto>> ListarArquivosAsync()
    {
        var arquivos = await _unitOfWork.Arquivos.GetAllOrderedAsync();
        return _mapper.Map<IEnumerable<ArquivoDto>>(arquivos);
    }

    public async Task<ArquivoDetalhadoDto?> ObterPorIdAsync(int id)
    {
        var arquivo = await _unitOfWork.Arquivos.GetByIdComTransacoesAsync(id);
        return arquivo == null ? null : _mapper.Map<ArquivoDetalhadoDto>(arquivo);
    }

    public async Task<EstatisticasDto> ObterEstatisticasAsync()
    {
        var estatisticas = await _unitOfWork.Arquivos.GetEstatisticasPorStatusAsync();

        var recepcionados = estatisticas.GetValueOrDefault(StatusArquivo.Recepcionado, 0);
        var naoRecepcionados = estatisticas.GetValueOrDefault(StatusArquivo.NaoRecepcionado, 0);
        var total = recepcionados + naoRecepcionados;

        return new EstatisticasDto
        {
            TotalArquivos = total,
            ArquivosRecepcionados = recepcionados,
            ArquivosNaoRecepcionados = naoRecepcionados,
            PercentualSucesso = total > 0 ? Math.Round((double)recepcionados / total * 100, 2) : 0
        };
    }

    public async Task<int> DeletarExpiradosAsync()
    {
        var expirados = await _unitOfWork.Arquivos.GetExpiradosAsync(DateTime.UtcNow);
        var count = 0;

        foreach (var arquivo in expirados)
        {
            await _fileStorageService.DeleteFileAsync(arquivo.CaminhoBackup);
            _unitOfWork.Arquivos.Delete(arquivo);
            count++;
        }

        await _unitOfWork.CommitAsync();
        return count;
    }

    public async Task DeletarArquivoAsync(int id)
    {
        var arquivo = await _unitOfWork.Arquivos.GetByIdAsync(id);
        if (arquivo == null)
        {
            throw new InvalidOperationException("Arquivo não encontrado");
        }

        await _fileStorageService.DeleteFileAsync(arquivo.CaminhoBackup);
        _unitOfWork.Arquivos.Delete(arquivo);
        await _unitOfWork.CommitAsync();
    }

    private string CalcularMD5(byte[] conteudo)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(conteudo);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private TipoAdquirente DeterminarTipoAdquirente(string empresa)
    {
        return empresa.Contains("UfCard", StringComparison.OrdinalIgnoreCase)
            ? TipoAdquirente.UfCard
            : TipoAdquirente.FagammonCard;
    }
}