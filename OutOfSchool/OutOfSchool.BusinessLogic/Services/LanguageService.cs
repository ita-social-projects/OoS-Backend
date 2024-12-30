using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;
public class LanguageService : ILanguageService
{
    private readonly IEntityRepository<long, Language> repository;
    private readonly ILogger<LanguageService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public LanguageService(IEntityRepository<long, Language> repository,
        ILogger<LanguageService> logger,
        IMapper mapper)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<LanguageDto>> GetAll()
    {
        logger.LogInformation("Getting all languages started.");

        var languages = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!languages.Any()
            ? "Languages table is empty."
            : $"All {languages.Count()} records were successfully received from the Languages table.");

        return languages.Select(mapper.Map<LanguageDto>).ToList();
    }
}
