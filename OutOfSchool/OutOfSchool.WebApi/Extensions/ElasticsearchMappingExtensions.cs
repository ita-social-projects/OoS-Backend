using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nest;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Models;

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
                .ForMember(dest => dest.Directions, opt => opt.MapFrom(src => src.Directions))
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
                        opt.MapFrom(a => new GeoLocation(a.Latitude, a.Longitude)));
            cfg.CreateMap<TeacherDTO, TeacherES>();
            cfg.CreateMap<DateTimeRangeDto, DateTimeRangeES>()
                .ForMember(dest => dest.Workdays, opt => opt.MapFrom(src => string.Join(' ', src.Workdays)));
            cfg.CreateMap<DirectionDto, DirectionES>();
        });
    }

    public static WorkshopFilterES ToESModel(this WorkshopFilter workshopFilterDto)
    {
        return Mapper<WorkshopFilter, WorkshopFilterES>(workshopFilterDto, cfg =>
        {
            cfg.CreateMap<WorkshopFilter, WorkshopFilterES>()
                .ForMember(dest => dest.Workdays, opt => opt.MapFrom(src => string.Join(' ', src.Workdays)));
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
                .ForMember(dest => dest.DirectionsId, opt => opt.MapFrom(src => src.Directions.Select(x => x.Id)));
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
                .ForMember(dest => dest.DirectionsId, opt => opt.MapFrom(src => src.Directions.Select(x => x.Id)));

            cfg.CreateMap<AddressES, AddressDto>()
                .ForMember(
                    dest => dest.Latitude,
                    opt =>
                        opt.MapFrom(src => src.Point.Latitude))
                .ForMember(
                    dest => dest.Longitude,
                    opt =>
                        opt.MapFrom(src => src.Point.Longitude));
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