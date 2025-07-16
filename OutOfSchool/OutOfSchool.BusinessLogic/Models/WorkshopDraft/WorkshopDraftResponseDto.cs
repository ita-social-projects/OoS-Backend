using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResponseDto
{
    public Guid WorkshopDraftId { get; set; }
    public WorkshopDraftStatus DraftStatus { get; set; }      
    public string? RejectionMessage { get; set; }
    public WorkshopV2Dto WorkshopDetails { get; set; }
    public string ProviderEdrpou { get; set; }
    public string DirectorFullName { get; set; }
    public string DirectorPosition { get; set; }
}

public static class WorkshopDraftResponseDtoExtensions
{
    public static WorkshopDraftResponseDto ToResponseDto(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft)        
    {
        var dto = new WorkshopDraftResponseDto()
        {
            WorkshopDraftId = draft.Id,
            DraftStatus = draft.DraftStatus,
            RejectionMessage = draft.RejectionMessage,
            WorkshopDetails = draft.ToDto(),
            ProviderEdrpou = draft.Provider?.Edrpou ?? string.Empty,
            DirectorFullName = string.Empty,
            DirectorPosition = string.Empty,
        };         
        
        var officials = draft.Provider?.Positions?.SelectMany(o => o.Officials).ToList();

        var director = officials?.FirstOrDefault(o => o.Position.PositionType == PositionType.Director);
        var deputy = officials?.FirstOrDefault(o => o.Position.PositionType == PositionType.DeputyDirector);

        if (director?.Individual is not null)
        {
            dto.DirectorFullName = $"{director.Individual.FirstName} {director.Individual.LastName} {director.Individual.MiddleName}".Trim();
            dto.DirectorPosition = PositionType.Director.ToString();
        }

        else if (deputy?.Individual is not null)
        {
            dto.DirectorFullName = $"{director.Individual.FirstName} {director.Individual.LastName} {director.Individual.MiddleName}".Trim();
            dto.DirectorPosition = PositionType.DeputyDirector.ToString();
        }

        return dto;
    }

    public static List<WorkshopDraftResponseDto> ToResponseDto(this IEnumerable<OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft> list)
        => list.MapToList(ToResponseDto);
}