using System;
using System.Linq;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Models.Workshops.V2;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Tests.Common;

// TODO: Need to refactor tests that use mappings in this file
public class TestMappingProfile : Profile
{
    public TestMappingProfile()
    {
        CreateMap<Provider, ProviderUpdateDto>()
            .Apply(AddCommonProvider2ProviderBaseDto);
        CreateMap<Provider, ProviderCreateDto>()
            .Apply(AddCommonProvider2ProviderBaseDto);
        CreateMap<Workshop, WorkshopV2CreateRequestDto>()
            .IncludeBase<Workshop, WorkshopCreateRequestDto>()
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());
        CreateMap<Workshop, WorkshopCreateRequestDto>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(Constants.MappingSeparator, StringSplitOptions.None)))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(
                    src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.DateTimeRanges,
                opt => opt.MapFrom(src => src.DateTimeRanges.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.WorkshopDescriptionItems,
                opt => opt.MapFrom(src => src.WorkshopDescriptionItems.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id).ToList()));
        CreateMap<Teacher, TeacherCreateDto>()
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());
        CreateMap<Teacher, TeacherUpdateDto>()
            .IncludeBase<Teacher, TeacherCreateDto>();
        CreateMap<Workshop, WorkshopUpdateDto>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(Constants.MappingSeparator, StringSplitOptions.None)))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(
                    src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.DateTimeRanges,
                opt => opt.MapFrom(src => src.DateTimeRanges.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.WorkshopDescriptionItems,
                opt => opt.MapFrom(src => src.WorkshopDescriptionItems.Where(x => !x.IsDeleted)));

        CreateMap<Workshop, WorkshopUpdateV2Dto>()
            .IncludeBase<Workshop, WorkshopUpdateDto>();

        CreateMap<WorkshopUpdateDto, WorkshopDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.TagIds.Select(id => new TagDto {Id = id}).ToList()))
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderOwnership, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderStatus, opt => opt.Ignore());

        CreateMap<WorkshopUpdateDto, IHasRating>();

        CreateMap<TeacherUpdateDto, TeacherDto>();
    }

    private IMappingExpression<Provider, T> AddCommonProvider2ProviderBaseDto<T>(
        IMappingExpression<Provider, T> mappings)
        where T : ProviderBaseDto
        => mappings
            .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(src => src.ActualAddress))
            .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(src => src.LegalAddress))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution))
            .Apply(IgnoreAllImages)
            .Apply(MapImageIds);

    private IMappingExpression<TSource, TDestination> IgnoreAllImages<TSource, TDestination>(
        IMappingExpression<TSource, TDestination> mappings)
        where TDestination : IHasCoverImage, IHasImages
        => mappings
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore());

    private IMappingExpression<TSource, TDestination> MapImageIds<TSource, TDestination>(
        IMappingExpression<TSource, TDestination> mappings)
        where TSource : class, IHasEntityImages<TSource>
        where TDestination : class, IHasImages
        => mappings
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)));
}