using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for the CompanyInformation entities (AboutPortal, SupportInformation and LawsAndRegulations).
/// </summary>
/// <param name="companyInformationRepository">CompanyInformation repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="mapper">Mapper.</param>
public class CompanyInformationService(
    ISensitiveEntityRepository<CompanyInformation> companyInformationRepository,
    ILogger<CompanyInformationService> logger
) : ICompanyInformationService
{
    private const int LimitOfItems = 10;
    private const int MainLimit = 1;

    /// <inheritdoc/>
    public async Task<CompanyInformationDto> GetByType(CompanyInformationType type)
    {
        logger.LogDebug("Get CompanyInformation is started.");

        var companyInformation = (await companyInformationRepository.GetByFilter(GetFilter(type), "CompanyInformationItems").ConfigureAwait(false)).FirstOrDefault();

        logger.LogDebug("Get CompanyInformation is finished.");

        return companyInformation.ToDto();
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
        var isMain = type == CompanyInformationType.Main;
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
            var items = companyInformationDto.CompanyInformationItems.ToModel();

            // Clear CompanyInformationItemId because we will replace items for the CompanyInformation (old items will be deleted automatically by the EF)
            foreach (var item in items)
            {
                item.Id = Guid.Empty;
            }

            companyInformation.CompanyInformationItems = items;
            companyInformation.Title = companyInformationDto.Title;
        }

        await companyInformationRepository.SaveChangesAsync().ConfigureAwait(false);

        return companyInformation.ToDto();
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
        return await companyInformationRepository.Create(companyInformationDto.ToModel()).ConfigureAwait(false);
    }
}