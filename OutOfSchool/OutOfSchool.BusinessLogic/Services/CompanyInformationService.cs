using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for the CompanyInformation entities (AboutPortal, SupportInformation and LawsAndRegulations).
/// </summary>
public class CompanyInformationService : ICompanyInformationService
{
    private const int LimitOfItems = 10;
    private const int MainLimit = 1;
    private readonly ISensitiveEntityRepository<CompanyInformation> companyInformationRepository;
    private readonly ILogger<CompanyInformationService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyInformationService"/> class.
    /// </summary>
    /// <param name="companyInformationRepository">CompanyInformation repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public CompanyInformationService(
        ISensitiveEntityRepository<CompanyInformation> companyInformationRepository,
        ILogger<CompanyInformationService> logger,
        IMapper mapper)
    {
        this.companyInformationRepository = companyInformationRepository ?? throw new ArgumentNullException(nameof(companyInformationRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<CompanyInformationDto> GetByType(CompanyInformationType type)
    {
        logger.LogDebug("Get CompanyInformation is started.");

        var companyInformation = (await companyInformationRepository.GetByFilter(GetFilter(type), "CompanyInformationItems").ConfigureAwait(false)).FirstOrDefault();

        logger.LogDebug("Get CompanyInformation is finished.");

        return mapper.Map<CompanyInformationDto>(companyInformation);
    }

    /// <inheritdoc/>
    public Task<CompanyInformationDto> Update(CompanyInformationDto companyInformationDto, CompanyInformationType type)
    {
        logger.LogDebug("Updating CompanyInformation is started.");
        if (companyInformationDto == null)
        {
            throw new ArgumentNullException(nameof(companyInformationDto));
        }

        var itemsCount = companyInformationDto.CompanyInformationItems.Count();
        bool isMain = type == CompanyInformationType.Main;
        if (itemsCount > LimitOfItems || (isMain && itemsCount > MainLimit))
        {
            throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
        }

        companyInformationDto.Type = type;

        var companyInformation = UpdateOrCreateAsync(companyInformationDto);

        logger.LogDebug("Updating CompanyInformation is finished.");

        return companyInformation;
    }

    private static Expression<Func<CompanyInformation, bool>> GetFilter(CompanyInformationType type)
    {
        return ci => ci.Type == type;
    }

    private async Task<CompanyInformationDto> UpdateOrCreateAsync(CompanyInformationDto companyInformationDto)
    {
        var companyInformation = (await companyInformationRepository.GetByFilter(GetFilter(companyInformationDto.Type), "CompanyInformationItems").ConfigureAwait(false)).FirstOrDefault();

        if (companyInformation == null)
        {
            companyInformation = await Create(companyInformationDto).ConfigureAwait(false);
        }
        else
        {
            var items = mapper.Map<List<CompanyInformationItem>>(companyInformationDto.CompanyInformationItems);

            // Clear CompanyInformationItemId because we will replace items for the CompanyInformation (old items will be deleted automatically by the EF)
            foreach (var item in items)
            {
                item.Id = Guid.Empty;
            }

            companyInformation.CompanyInformationItems = items;
            companyInformation.Title = companyInformationDto.Title;
        }

        await companyInformationRepository.SaveChangesAsync().ConfigureAwait(false);

        return mapper.Map<CompanyInformationDto>(companyInformation);
    }

    private Task<CompanyInformation> Create(CompanyInformationDto companyInformationDto)
    {
        logger.LogDebug("CompanyInformation creating is started.");

        if (companyInformationDto == null)
        {
            throw new ArgumentNullException(nameof(companyInformationDto));
        }

        var companyInformation = CreateAsync(companyInformationDto);

        logger.LogDebug($"CompanyInformation with Id = {companyInformation?.Id} created successfully.");

        return companyInformation;
    }

    private async Task<CompanyInformation> CreateAsync(CompanyInformationDto companyInformationDto)
    {
        return await companyInformationRepository.Create(mapper.Map<CompanyInformation>(companyInformationDto)).ConfigureAwait(false);
    }
}