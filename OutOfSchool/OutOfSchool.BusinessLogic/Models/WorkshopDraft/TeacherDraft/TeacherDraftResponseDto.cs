using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;

public class TeacherDraftResponseDto : TeacherDraftDto
{
    public Guid Id { get; set; }

    public Guid WorkshopDraftId { get; set; }

    public string CoverImageId { get; set; }
}

public static class TeacherDraftResponseDtoExtensions
{
    public static TeacherDraftResponseDto ToResponseDto(this OutOfSchool.Services.Models.WorkshopDrafts.TeacherDraft draft)
        => new() 
        { 
            FirstName = draft.FirstName,
            LastName = draft.LastName,
            MiddleName = draft.MiddleName,
            Gender = draft.Gender,
            DateOfBirth = draft.DateOfBirth,
            Description = draft.Description,
            IsDefaultTeacher = draft.IsDefaultTeacher,
            Id = draft.Id,
            WorkshopDraftId = draft.WorkshopDraftId,
            CoverImageId = draft.CoverImageId,
        };
}
