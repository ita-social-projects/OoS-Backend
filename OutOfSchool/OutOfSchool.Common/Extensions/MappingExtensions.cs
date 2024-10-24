using System;
using AutoMapper;

namespace OutOfSchool.Common.Extensions;

public static class MappingExtensions
{
    public static IMappingExpression<TSource, TDestination> Apply<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> mappings,
        Func<IMappingExpression<TSource, TDestination>, IMappingExpression<TSource, TDestination>> addMappings
    )
        where TSource : class
        where TDestination : class
        => addMappings(mappings);

    public static IMapperConfigurationExpression UseProfile<T>(this IMapperConfigurationExpression cfg)
        where T : Profile, new()
    {
        cfg.AddProfile<T>();
        return cfg;
    }
}