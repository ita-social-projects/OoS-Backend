using System;
using AutoMapper;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Models;

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

    public static IMappingExpression<TSource, TDestination> ApplyDefaultsForHiddenFields<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map)
        where TSource : class
        where TDestination : class, IHasHiddenFields
    {
        return map
            .ForMember(dest => dest.IsSelfFinanced, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.IsInclusive, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.SpecialNeedsType, opt => opt.MapFrom(_ => SpecialNeedsType.None))
            .ForMember(dest => dest.EducationalShift, opt => opt.MapFrom(_ => EducationalShift.First))
            .ForMember(dest => dest.AgeComposition, opt => opt.MapFrom(_ => AgeComposition.SameAge));
    }
}