using System;
using System.Linq;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Tests.Common;

// This mapping profile is created to extract wrong mappings that were created to make testing easier
// It does not include Elastic search mappings, need to review those separately
// TODO: Need to refactor tests that use mappings in this file
public class TestMappingProfile : Profile
{
    public const char MappingSeparator = '�';

    public TestMappingProfile()
    {
        // User in OutOfSchool.WebApi.IntegrationTests.ProviderServiceIntergrationTests.
        // Used in OutOfSchool.WebApi.Tests.Controllers.ProviderControllersTests.ProviderControllerTests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceV2Tests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceTests
        CreateMap<Provider, ProviderUpdateDto>()
            .Apply(AddCommonProvider2ProviderBaseDto);
        // Used in OutOfSchool.WebApi.Tests.Controllers.ProviderControllersTests.ProviderControllerTests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceV2Tests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceTests
        CreateMap<Provider, ProviderCreateDto>()
            .Apply(AddCommonProvider2ProviderBaseDto);
        
        // Used in OutOfSchool.WebApi.Tests.Controllers.WorkshopControllerV2Tests
        CreateMap<Workshop, WorkshopV2CreateRequestDto>()
            .IncludeBase<Workshop, WorkshopCreateRequestDto>()
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());

        // Used in OutOfSchool.WebApi.Tests.Services.WorkshopServicesCombinerTests
        CreateMap<WorkshopCreateUpdateDto, IHasRating>();
        
        // Used in OutOfSchool.WebApi.Tests.Services.WorkshopServiceTests
        CreateMap<Workshop, WorkshopCreateUpdateDto>()
            .IncludeBase<Workshop, WorkshopBaseDto>()
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id).ToList()));
        
        // Used in OutOfSchool.WebApi.Tests.Services.WorkshopServicesCombinerTests
        CreateMap<WorkshopCreateUpdateDto, WorkshopDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.TagIds.Select(id => new TagDto {Id = id}).ToList()))
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderOwnership, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderStatus, opt => opt.Ignore())
        
            // TODO: Remove
            .ForMember(dest => dest.Phone, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.Website, opt => opt.Ignore())
            .ForMember(dest => dest.Facebook, opt => opt.Ignore())
            .ForMember(dest => dest.Instagram, opt => opt.Ignore())
            .ForMember(dest => dest.Address, opt => opt.Ignore());

        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceTests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceV2Tests
        CreateSoftDeletedMap<ProviderDto, Provider>()
            .Apply(IgnoreCommonProviderBaseDto2Provider)
            .ForMember(dest => dest.Workshops, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Positions, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopDrafts, opt => opt.Ignore());

        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceTests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceV2Tests
        CreateMap<ProviderDto, ProviderUpdateDto>();

        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceTests
        // Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceV2Tests
        CreateMap<ProviderCreateDto, ProviderDto>()
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.BlockReason, opt => opt.Ignore())
            .IncludeBase<object, IHasRating>()
            .ForMember(dest => dest.BlockPhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore());

        // TODO: These impossible mappings are not used in tests. Leave commented for one PR
        // TODO: Remove only if they are not used in real code
        // CreateMap<Individual, UploadEmployeeRequestDto>()
        //     .ForMember(dest => dest.AssignedRole, opt => opt.Ignore());
        // CreateMap<SocialGroup, SocialGroupCreate>();
        // CreateMap<WorkshopDto, Workshop>()
        //     .IncludeBase<WorkshopBaseDto, Workshop>();
        // CreateMap<WorkshopV2Dto, Workshop>()
        //     .IncludeBase<WorkshopDto, Workshop>();

        //Used in OutOfSchool.WebApi.IntegrationTests.ProviderServiceIntergrationTests.ProviderServiceUpdate
        //Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceV2Tests
        //Used in OutOfSchool.WebApi.Tests.Services.ProviderServicesTests.ProviderServiceTests
        //Used in OutOfSchool.WebApi.Tests.Services.WorkshopServiceTests
        //Used in OutOfSchool.WebApi.Tests.Services.WorkshopServicesCombinerV2Tests
        //Used in OutOfSchool.WebApi.Tests.Services.WorkshopServicesCombinerTests
        //Used in OutOfSchool.WebApi.Tests.Services.ProviderServiceTests
        //Used in OutOfSchool.WebApi.Tests.Controllers.WorkshopControllerV2Tests
        //Used in OutOfSchool.WebApi.Tests.Controllers.WorkshopControllerTests
        //Used in OutOfSchool.WebApi.Tests.Controllers.ProviderControllersTests.ProviderControllerTests
        CreateMap<WorkshopDto, WorkshopCreateUpdateDto>()
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id).ToList()));
        CreateMap<Workshop, WorkshopCreateRequestDto>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(MappingSeparator, StringSplitOptions.None)))
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
        CreateMap<Workshop, WorkshopV2CreateRequestDto>()
            .IncludeBase<Workshop, WorkshopCreateRequestDto>()
            .ForMember(dest => dest.ImageIds,
                opt => opt.MapFrom(src => src.Images.Select(w => w.ExternalStorageId).ToList()))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(src => src.CoverImageId))
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());
    }

    private IMappingExpression<Provider, T> AddCommonProvider2ProviderBaseDto<T>(
        IMappingExpression<Provider, T> mappings)
        where T : ProviderBaseDto
        => mappings
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts))
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

    public IMappingExpression<TSource, TDestination> CreateSoftDeletedMap<TSource, TDestination>()
        where TSource : class
        where TDestination : class, ISoftDeleted
        => CreateMap<TSource, TDestination>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

    private IMappingExpression<T, Provider> IgnoreCommonProviderBaseDto2Provider<T>(
        IMappingExpression<T, Provider> mappings)
        where T : ProviderBaseDto
        => mappings
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusReason, opt => opt.Ignore())
            .ForMember(dest => dest.LicenseStatus, opt => opt.Ignore());
}