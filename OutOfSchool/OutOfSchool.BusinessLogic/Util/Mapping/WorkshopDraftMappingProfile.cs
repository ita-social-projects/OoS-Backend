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
            .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => string.Join(Constants.MappingSeparator, src.Keywords)));

        CreateMap<WorkshopDraft, WorkshopDraftResponseDto>()
            .IncludeMembers(src => src.WorkshopDraftContent)
            .ForMember(dest => dest.ImagesIds, opt => opt.MapFrom(
                src => src.Images.Select(x => x.ExternalStorageId)
                .ToList()));

        CreateMap<WorkshopV2Dto, WorkshopDraftContent>();
        CreateMap<WorkshopV2Dto, WorkshopDraft>()
            .ForPath(dest => dest.WorkshopDraftContent, opt => opt.MapFrom(src => src))
            .ForPath(dest => dest.WorkshopDraftContent.TagIds, opt => opt.MapFrom(src => src.Tags.Select(t => t.Id).Concat(src.TagIds)))
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DraftStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(src => src.Id == Guid.Empty ? (Guid?) null : src.Id));

        CreateMap<WorkshopDraftContent, WorkshopV2Dto>();
        CreateMap<WorkshopDraft, WorkshopV2Dto>()
            .IncludeMembers(src => src.WorkshopDraftContent)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WorkshopId));

        CreateMap<WorkshopDraftContent, WorkshopV2CreateRequestDto>();
        CreateMap<WorkshopDraft, WorkshopV2CreateRequestDto>()
            .IncludeMembers(src => src.WorkshopDraftContent)
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<DateTimeRangeDraft, DateTimeRangeDto>();

        CreateMap<DateTimeRangeDto, DateTimeRangeDraft>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.EndTime)));

        CreateMap<DateTimeRangeDraft, DateTimeRangeDto>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToTimeSpan()))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToTimeSpan()));

        CreateMap<WorkshopDescriptionItemDraft, WorkshopDescriptionItemDto>()
            .ReverseMap();

        CreateMap<AddressDraft, AddressDto>()
            .ReverseMap();

        CreateMap<TeacherDraft, TeacherDTO>()
            .ReverseMap();
    }
}
