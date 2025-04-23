namespace OutOfSchool.BusinessLogic.Models.StudySubjects;
public class StudySubjectDto
{
    public Guid Id { get; set; }
    public string NameInUkrainian { get; set; }
    public string NameInInstructionLanguage { get; set; }
    public bool IsLanguageUkrainian { get; set; }
    public long LanguageId { get; set; }
    public LanguageDto Language { get; set; }
    public Guid ProviderId { get; set; }
    public List<ShortEntityDto> Workshops { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}

public static class StudySubjectDtoExtensions
{
    public static StudySubjectDto ToDto(this StudySubject model)
        => new()
        {
            Id = model.Id,
            NameInUkrainian = model.NameInUkrainian,
            NameInInstructionLanguage = model.NameInInstructionLanguage,
            IsLanguageUkrainian= model.IsLanguageUkrainian,
            LanguageId = model.LanguageId,
            Language = model.Language.ToDto(),
            ProviderId = model.ProviderId,
            Workshops = model.Workshops.ToShortEntityDto()
        };

    public static List<StudySubjectDto> ToDto(this IEnumerable<StudySubject> list)
        => list.MapToList(ToDto);
}
