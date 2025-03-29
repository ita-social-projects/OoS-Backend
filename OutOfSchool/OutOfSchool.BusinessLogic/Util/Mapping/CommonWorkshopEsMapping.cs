using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Util.Mapping;

public static class CommonWorkshopEsMapping
{
    public static IMappingExpression<TSource, WorkshopES> CommonFieldsMapping<TSource>(this IMappingExpression<TSource, WorkshopES> mapper)
        where TSource : WorkshopDto
    {
        return mapper
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchyId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
            .ForMember(dest => dest.ProviderOwnership, opt => opt.MapFrom(src => src.ProviderOwnership))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
            .ForMember(dest => dest.NumberOfRatings, opt => opt.MapFrom(src => src.NumberOfRatings))
            .ForMember(dest => dest.ProviderStatus, opt => opt.MapFrom(src => src.ProviderStatus))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.TakenSeats, opt => opt.MapFrom(src => src.TakenSeats))
            .ForMember(dest => dest.IsSelfFinanced, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.IsInclusive, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.SpecialNeedsType, opt => opt.MapFrom(_ => SpecialNeedsType.None))
            .ForMember(dest => dest.EducationalShift, opt => opt.MapFrom(_ => EducationalShift.First))
            .ForMember(dest => dest.AgeComposition, opt => opt.MapFrom(_ => AgeComposition.SameAge));
    }
}