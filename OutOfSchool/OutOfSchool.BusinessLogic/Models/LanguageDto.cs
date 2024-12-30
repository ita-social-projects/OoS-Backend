namespace OutOfSchool.BusinessLogic.Models;
public class LanguageDto
{
    public int Id { get; set; }

    /// <summary>
    /// ISO code of the language
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Title of the language
    /// </summary>
    public string Title { get; set; }
}
