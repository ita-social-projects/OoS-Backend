using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ElasticsearchMappingExtensions
    {
        // From DTO to ES models
        public static WorkshopES ToESModel(this WorkshopDTO workshopDto)
        {
            return Mapper<WorkshopDTO, WorkshopES>(workshopDto, cfg =>
            {
                cfg.CreateMap<WorkshopDTO, WorkshopES>()
                    .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => string.Join('¤', src.Keywords.Distinct())));
                cfg.CreateMap<AddressDto, AddressES>();
                cfg.CreateMap<TeacherDTO, TeacherES>();
            });
        }

        public static WorkshopFilterES ToESModel(this WorkshopFilter workshopFilterDto)
        {
            return Mapper<WorkshopFilter, WorkshopFilterES>(workshopFilterDto, cfg =>
            {
                cfg.CreateMap<WorkshopFilter, WorkshopFilterES>();
            });
        }

        // From ES models to DTO
        public static WorkshopCard ToCardDto(this WorkshopES workshopES)
        {
            return Mapper<WorkshopES, WorkshopCard>(workshopES, cfg =>
            {
                cfg.CreateMap<WorkshopES, WorkshopCard>()
                    .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(dest => dest.Photo, opt => opt.MapFrom(s => s.Logo));
                cfg.CreateMap<AddressES, AddressDto>();
            });
        }

        public static SearchResult<WorkshopCard> ToSearchResult(this SearchResultES<WorkshopES> searchResult)
        {
            return Mapper<SearchResultES<WorkshopES>, SearchResult<WorkshopCard>>(searchResult, cfg =>
            {
                cfg.CreateMap<SearchResultES<WorkshopES>, SearchResult<WorkshopCard>>();
                cfg.CreateMap<WorkshopES, WorkshopCard>()
                    .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(dest => dest.Photo, opt => opt.MapFrom(s => s.Logo));
                cfg.CreateMap<AddressES, AddressDto>();
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
}
