using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Workshops;

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
            .ForMember(dest => dest.TakenSeats, opt => opt.MapFrom(src => src.TakenSeats));
    }
}