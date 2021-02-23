using System;
using AutoMapper;

namespace OutOfSchool.WebApi.Mapping.Extensions
{
    public static class MapExtension
    {

        public static TDestination Map<TSource, TDestination>(this TSource source, Action<IMapperConfigurationExpression> configure)
        {
            var config = new MapperConfiguration(configure);
            var mapper = config.CreateMapper();
            var destination = mapper.Map<TDestination>(source);
            return destination;
        }

        public static TDestination Map<TSource, TDestination>(this TSource source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>().MaxDepth(1);
            });
            var mapper = config.CreateMapper();
            var destination = mapper.Map<TDestination>(source);
            return destination;
        }


        public static TDestination Map<TSource, TDestination>(this TSource source, TDestination destination)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>().MaxDepth(1);
            });
            var mapper = config.CreateMapper();
            return mapper.Map<TSource, TDestination>(source, destination);
        }
    }
}