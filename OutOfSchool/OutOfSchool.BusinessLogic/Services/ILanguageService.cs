using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;
public interface ILanguageService
{
    /// <summary>
    /// Get all languages.
    /// </summary>
    /// <returns>List of all languages.</returns>
    Task<IEnumerable<LanguageDto>> GetAll();
}
