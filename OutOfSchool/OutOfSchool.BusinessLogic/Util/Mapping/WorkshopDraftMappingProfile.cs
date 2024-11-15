using AutoMapper;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Util.Mapping;
public class WorkshopDraftMappingProfile : Profile
{
  public WorkshopDraftMappingProfile()
  {
        CreateMap<DateTimeRangeDraft, DateTimeRangeDraftDto>().ReverseMap();
        CreateMap<WorkshopDescriptionItemDraft, WorkshopDescriptionItemDraftDto>().ReverseMap();

        CreateMap<WorkshopDraftContent, WorkshopDraftContentDto>()
    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
    .ForMember(dest => dest.ShortTitle, opt => opt.MapFrom(src => src.ShortTitle))
    .ForMember(dest => dest.MinAge, opt => opt.MapFrom(src => src.MinAge))
    .ForMember(dest => dest.MaxAge, opt => opt.MapFrom(src => src.MaxAge))
    .ForMember(dest => dest.DateTimeRange, opt => opt.MapFrom(src => src.DateTimeRange))
    .ForMember(dest => dest.WorkshopDescriptionItems, opt => opt.MapFrom(src => src.WorkshopDescriptionItems))
    .ForMember(dest => dest.CompetitiveSelection, opt => opt.MapFrom(src => src.CompetitiveSelection))
    .ForMember(dest => dest.CompetitiveSelectionDescription, opt => opt.MapFrom(src => src.CompetitiveSelectionDescription))
    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
    .ForMember(dest => dest.PayRate, opt => opt.MapFrom(src => src.PayRate))
    .ForMember(dest => dest.FormOfLearning, opt => opt.MapFrom(src => src.FormOfLearning))
    .ForMember(dest => dest.TotalSeats, opt => opt.MapFrom(src => src.TotalSeats))
    .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds))
    .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Keywords))
    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
    .ForMember(dest => dest.Facebook, opt => opt.MapFrom(src => src.Facebook))
    .ForMember(dest => dest.Instagram, opt => opt.MapFrom(src => src.Instagram))
    .ForMember(dest => dest.IsSelfFinanced, opt => opt.MapFrom(src => src.IsSelfFinanced))
    .ForMember(dest => dest.IsSpecial, opt => opt.MapFrom(src => src.IsSpecial))
    .ForMember(dest => dest.IsInclusive, opt => opt.MapFrom(src => src.IsInclusive))
    .ForMember(dest => dest.ActiveFrom, opt => opt.MapFrom(src => src.ActiveFrom))
    .ForMember(dest => dest.ActiveTo, opt => opt.MapFrom(src => src.ActiveTo))
    .ForMember(dest => dest.InstitutionHierarchyId, opt => opt.MapFrom(src => src.InstitutionHierarchyId))
    .ForMember(dest => dest.EducationalShiftId, opt => opt.MapFrom(src => src.EducationalShiftId));

        CreateMap<WorkshopDraftCreateDto, WorkshopDraft>()
    .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.ProviderId))
    .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<WorkshopDraftContentDto, WorkshopDraftContent>()
    .ForMember(dest => dest.MinAge, opt => opt.MapFrom(src => src.MinAge))
    .ForMember(dest => dest.MaxAge, opt => opt.MapFrom(src => src.MaxAge))
    .ForMember(dest => dest.DateTimeRange, opt => opt.MapFrom(src => src.DateTimeRange))
    .ForMember(dest => dest.WorkshopDescriptionItems, opt => opt.MapFrom(src => src.WorkshopDescriptionItems))
    .ForMember(dest => dest.CompetitiveSelection, opt => opt.MapFrom(src => src.CompetitiveSelection))
    .ForMember(dest => dest.CompetitiveSelectionDescription, opt => opt.MapFrom(src => src.CompetitiveSelectionDescription))
    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
    .ForMember(dest => dest.PayRate, opt => opt.MapFrom(src => src.PayRate))
    .ForMember(dest => dest.FormOfLearning, opt => opt.MapFrom(src => src.FormOfLearning))
    .ForMember(dest => dest.TotalSeats, opt => opt.MapFrom(src => src.TotalSeats))
    .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds))
    .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Keywords))
    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
    .ForMember(dest => dest.Facebook, opt => opt.MapFrom(src => src.Facebook))
    .ForMember(dest => dest.Instagram, opt => opt.MapFrom(src => src.Instagram))
    .ForMember(dest => dest.IsSelfFinanced, opt => opt.MapFrom(src => src.IsSelfFinanced))
    .ForMember(dest => dest.IsSpecial, opt => opt.MapFrom(src => src.IsSpecial))
    .ForMember(dest => dest.IsInclusive, opt => opt.MapFrom(src => src.IsInclusive))
    .ForMember(dest => dest.ActiveFrom, opt => opt.MapFrom(src => src.ActiveFrom))
    .ForMember(dest => dest.ActiveTo, opt => opt.MapFrom(src => src.ActiveTo))
    .ForMember(dest => dest.InstitutionHierarchyId, opt => opt.MapFrom(src => src.InstitutionHierarchyId));
  
        CreateMap<WorkshopDraft, WorkshopDraftResponseDto>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.ProviderId))
    .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(src => src.CoverImageId))
    .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(img => img.ExternalStorageId)))
    .ForMember(dest => dest.DraftStatus, opt => opt.MapFrom(src => src.DraftStatus));
  }
}
