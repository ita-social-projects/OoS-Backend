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
