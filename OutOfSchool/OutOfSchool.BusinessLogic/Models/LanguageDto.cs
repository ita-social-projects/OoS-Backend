namespace OutOfSchool.BusinessLogic.Models;
public class LanguageDto
{
    public long Id { get; set; }

    /// <summary>
    /// ISO code of the language
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Name of the language
    /// </summary>
    public string Name { get; set; }
}

public static class LanguageDtoExtensions
{
    public static Language ToModel(this LanguageDto dto)
        => new()
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
        };

    public static LanguageDto ToDto(this Language model)
        => new()
        {
            Id = model.Id,
            Code = model.Code,
            Name = model.Name,
        };

    public static List<LanguageDto> ToDto(this IEnumerable<Language> list)
        => list.MapToList(ToDto);
}
