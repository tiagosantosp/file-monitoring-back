using AutoMapper;
using FileMonitoring.Application.DTOs;
using FileMonitoring.Domain.Entities;

namespace FileMonitoring.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Arquivo, ArquivoDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TipoAdquirente, opt => opt.MapFrom(src => src.TipoAdquirente.ToString()))
            .ForMember(dest => dest.QuantidadeTransacoes, opt => opt.MapFrom(src => src.Transacoes.Count));

        CreateMap<Arquivo, ArquivoDetalhadoDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TipoAdquirente, opt => opt.MapFrom(src => src.TipoAdquirente.ToString()));

        CreateMap<TransacaoArquivo, TransacaoArquivoDto>()
            .ForMember(dest => dest.TipoRegistro, opt => opt.MapFrom(src => src.TipoRegistro.ToString()));
    }
}