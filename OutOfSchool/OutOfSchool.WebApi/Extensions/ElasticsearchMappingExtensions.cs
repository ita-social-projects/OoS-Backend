using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nest;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.Common.Models;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Codeficator;

namespace OutOfSchool.WebApi.Extensions;

// TODO: create a mapping profile instead of extensions
// TODO: Do it sooner then later as it leads to further inconsistencies
public static class ElasticsearchMappingExtensions
{
    private const char SEPARATOR = '¤';

    // From DTO to ES models
    public static WorkshopES ToESModel(this WorkshopDTO workshopDto)
    {
        return Mapper<WorkshopDTO, WorkshopES>(workshopDto, cfg =>
        {
            cfg.CreateMap<WorkshopDTO, WorkshopES>()
                .ForMember(
                    dest => dest.Keywords,
                    opt =>
                        opt.MapFrom(src =>
                            string.Join(SEPARATOR, src.Keywords.Distinct())))
                .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds))
                .ForMember(
                    dest => dest.Description,
                    opt =>
                        opt.MapFrom(src =>
                            src.WorkshopDescriptionItems
                                .Aggregate(string.Empty, (accumulator, wdi) =>
                                    $"{accumulator}{wdi.SectionName}{SEPARATOR}{wdi.Description}{SEPARATOR}")));

            cfg.CreateMap<AddressDto, AddressES>()
                .ForMember(
                    dest => dest.Point,
                    opt =>
                        opt.MapFrom(a => new GeoLocation(a.Latitude, a.Longitude)))
                .ForMember(
                    dest => dest.CodeficatorAddressES,
                    opt => opt.MapFrom(c => c.CodeficatorAddressDto));
            cfg.CreateMap<AllAddressPartsDto, CodeficatorAddressES>()
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.AddressParts.ParentId));
            cfg.CreateMap<DateTimeRangeDto, DateTimeRangeES>()
                .ForMember(dest => dest.Workdays, opt => opt.MapFrom(src => string.Join(' ', src.Workdays)));
        });
    }

    public static ProviderStatusDto ToProviderStatusDto(this Provider provider)
    {
        return Mapper<Provider, ProviderStatusDto>(provider, ctg => 
            ctg.CreateMap<Provider, ProviderStatusDto>()
                .ForMember(
                    dest => dest.ProviderId,
                    opt => 
                        opt.MapFrom(src=> src.Id))
                .ForMember(
                    dest => dest.Status,
                    opt => 
                        opt.MapFrom(src=> src.Status))
                .ForMember(
                    dest => dest.StatusReason,
                    opt => 
                        opt.MapFrom(src=> String.Empty))
        );
    }
    public static WorkshopFilterES ToESModel(this WorkshopFilter workshopFilterDto)
    {
        return Mapper<WorkshopFilter, WorkshopFilterES>(workshopFilterDto, cfg =>
        {
            cfg.CreateMap<WorkshopFilter, WorkshopFilterES>()
                .ForMember(dest => dest.Workdays, opt => opt.MapFrom(src => string.Join(' ', src.Workdays)))
                .ForMember(dest => dest.ElasticRadius, opt => opt.MapFrom(src => $"{src.RadiusKm * 1000}m"));
        });
    }

    // From ES models to DTO
    public static WorkshopCard ToCardDto(this WorkshopES workshopES)
    {
        return Mapper<WorkshopES, WorkshopCard>(workshopES, cfg =>
        {
            cfg.CreateMap<WorkshopES, WorkshopCard>()
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
                .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds));
            cfg.CreateMap<AddressES, AddressDto>()
                .ForMember(
                    dest => dest.Latitude,
                    opt => opt.MapFrom(src => src.Point.Latitude))
                .ForMember(
                    dest => dest.Longitude,
                    opt => opt.MapFrom(src => src.Point.Longitude));
        });
    }

    public static SearchResult<WorkshopCard> ToSearchResult(this SearchResultES<WorkshopES> searchResult)
    {
        return Mapper<SearchResultES<WorkshopES>, SearchResult<WorkshopCard>>(searchResult, cfg =>
        {
            cfg.CreateMap<SearchResultES<WorkshopES>, SearchResult<WorkshopCard>>();
            cfg.CreateMap<WorkshopES, WorkshopCard>()
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
                .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds));

            cfg.CreateMap<AddressES, AddressDto>()
                .ForMember(
                    dest => dest.Latitude,
                    opt =>
                        opt.MapFrom(src => src.Point.Latitude))
                .ForMember(
                    dest => dest.Longitude,
                    opt =>
                        opt.MapFrom(src => src.Point.Longitude))
                .ForMember(
                    dest => dest.CodeficatorAddressDto,
                    opt => opt.MapFrom(src => src.CodeficatorAddressES));

            cfg.CreateMap<CodeficatorAddressES, AllAddressPartsDto>()
                .ForMember(
                    dest => dest.AddressParts,
                    opt => opt.MapFrom(src => src));
            cfg.CreateMap<CodeficatorAddressES, CodeficatorDto>();
        });
    }

    private static TDestination Mapper<TSource, TDestination>(
        this TSource source,
        Action<IMapperConfigurationExpression> configure)
    {
        var config = new MapperConfiguration(configure);
        var mapper = config.CreateMapper();
        var destination = mapper.Map<TDestination>(source);
        return destination;
    }
}