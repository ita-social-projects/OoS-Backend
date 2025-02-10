using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.AddressDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Util.Mapping;
public class WorkshopDraftMappingProfile : Profile
{
    // TODO: After implementing the new model for Workshops, use the Workshop DTO model instead of the WorkshopDraft DTO.
    public WorkshopDraftMappingProfile()
    {
        // Nested entities that do not map to a database table
        CreateMap<WorkshopDescriptionItemDraftDto, WorkshopDescriptionItemDraft>()
            .ReverseMap();

        CreateMap<DateTimeRangeDraftDto, DateTimeRangeDraft>()            
            .ReverseMap();

        CreateMap<AddressDraftDto, AddressDraft>()
            .ReverseMap();

        CreateMap<TeacherDraftCreateDto, TeacherDraft>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId,opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopDraft, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopDraftId, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore());

        CreateMap<TeacherDraft, TeacherDraftResponseDto>();

        CreateMap<WorkshopDraftContent, WorkshopDraftResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.DraftStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ImagesIds, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.Ignore())
            .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => string.Join(Constants.MappingSeparator, src.Keywords)))
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());

        CreateMap<WorkshopDraft, WorkshopDraftResponseDto>()
            .IncludeMembers(src => src.WorkshopDraftContent)
            .ForMember(dest => dest.ImagesIds, opt => opt.MapFrom(
                src => src.Images.Select(x => x.ExternalStorageId)
                .ToList()))
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());

        CreateMap<WorkshopV2Dto, WorkshopDraftContent>()
            .ForMember(dest => dest.OwnershipType, opt => opt.MapFrom(src => src.ProviderOwnership))
            .ForMember(dest => dest.WorkshopStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IncludedStudyGroupsIds, opt => opt.MapFrom(src => src.IncludedStudyGroups.Select(g => g.Id)));          

        CreateMap<WorkshopV2Dto, WorkshopDraft>()
            .ForPath(dest => dest.WorkshopDraftContent, opt => opt.MapFrom(src => src))
            .ForPath(dest => dest.WorkshopDraftContent.TagIds, opt => opt.MapFrom(src => src.Tags.Select(t => t.Id).Concat(src.TagIds)))
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DraftStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionMessage, opt => opt.Ignore())
            .ForMember(dest => dest.Workshop, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(src => src.Id == Guid.Empty ? (Guid?) null : src.Id));

        CreateMap<WorkshopDraftContent, WorkshopV2Dto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.AddressId, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.WorkshopStatus))
            .ForMember(dest => dest.ProviderOwnership, opt => opt.MapFrom(src => src.OwnershipType))
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.IncludedStudyGroups, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultTeacher, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultTeacherId, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ParentWorkshop, opt => opt.Ignore())
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.Ignore());


        CreateMap<WorkshopDraft, WorkshopV2Dto>()
            .IncludeMembers(src => src.WorkshopDraftContent)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WorkshopId))
            .ForMember(dest => dest.AddressId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.IncludedStudyGroups, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultTeacherId, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(t => !t.IsDefaultTeacher)))
            .ForMember(dest => dest.DefaultTeacher,
                opt => opt.MapFrom(src => src.Teachers.FirstOrDefault(t => t.IsDefaultTeacher)))
            .ForMember(dest => dest.ParentWorkshop, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.Ignore());

        CreateMap<WorkshopDraftContent, WorkshopV2CreateRequestDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AddressId, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultTeacher, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultTeacherId, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.Ignore());

        CreateMap<WorkshopDraft, WorkshopV2CreateRequestDto>()
            .IncludeMembers(src => src.WorkshopDraftContent)
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AddressId, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultTeacherId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(t => !t.IsDefaultTeacher)))
            .ForMember(dest => dest.DefaultTeacher, 
                opt => opt.MapFrom(src => src.Teachers.FirstOrDefault(t => t.IsDefaultTeacher)))
            .ForMember(dest => dest.Contacts, opt => opt.Ignore());

        CreateMap<DateTimeRangeDto, DateTimeRangeDraft>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.EndTime)));

        CreateMap<DateTimeRangeDraft, DateTimeRangeDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToTimeSpan()))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToTimeSpan()));

        CreateMap<WorkshopDescriptionItemDraft, WorkshopDescriptionItemDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<AddressDraft, AddressDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CodeficatorAddressDto, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<TeacherDraft, TeacherDTO>()
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.Ignore())
            .ReverseMap();
    }
}
