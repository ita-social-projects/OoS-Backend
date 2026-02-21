using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;
public interface ILanguageService
{
    /// <summary>
    /// Get all languages.
    /// </summary>
    /// <returns>List of all languages.</returns>
    Task<IEnumerable<LanguageDto>> GetAll();

    /// <summary>
    /// Gets a language by its ID.
    /// </summary>
    /// <param name="id">Language ID.</param>
    /// <returns>Language DTO if found, null otherwise.</returns>
    Task<LanguageDto?> GetById(long id);
}
