using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;
/// <summary>
/// Initializes a new instance of the <see cref="LanguageService"/> class.
/// </summary>
/// <param name="repository">Repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="mapper">Mapper.</param>
/// <exception cref="ArgumentNullException"></exception>
public class LanguageService(
    IEntityRepository<long, Language> repository,
    ILogger<LanguageService> logger
) : ILanguageService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<LanguageDto>> GetAll()
    {
        logger.LogDebug("Getting all languages started");

        var languages = await repository.GetAll().ConfigureAwait(false);

        logger.LogDebug("{Count} records were successfully received from the Languages table", languages.Count());

        return languages.ToDto();
    }

    /// <inheritdoc/>
    public async Task<LanguageDto?> GetById(long id)
    {
        logger.LogDebug("Getting Language with ID = {LanguageId}.", id);

        var language = await repository.GetById(id).ConfigureAwait(false);

        if (language is null)
        {
            logger.LogWarning("Language with ID = {LanguageId} not found.", id);
            return null;
        }

        return language.ToDto();
    }
}
