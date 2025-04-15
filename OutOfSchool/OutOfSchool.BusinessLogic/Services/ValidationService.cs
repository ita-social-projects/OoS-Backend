using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class ValidationService : IValidationService
{
    private readonly IParentRepository parentRepository;
    private readonly IOfficialRepository officialRepository;

    public ValidationService(
        IParentRepository parentRepository,
        IOfficialRepository officialRepository)
    {
        this.parentRepository = parentRepository;
        this.officialRepository = officialRepository;
    }

    /// <inheritdoc/>>
    public async Task<bool> UserIsParentOwnerAsync(string userId, Guid parentId)
    {
        var parents = await parentRepository.GetByFilter(item => item.Id == parentId).ConfigureAwait(false);
        var parent = parents.SingleOrDefault();

        return parent is not null && userId.Equals(parent.UserId, StringComparison.Ordinal);
    }

    /// <inheritdoc/>>
    public async Task<Guid> GetParentOrProviderIdByUserRoleAsync(string userId, Role userRole)
    {
        if (userRole == Role.Parent)
        {
            var parents = await parentRepository.GetByFilter(item => item.UserId == userId).ConfigureAwait(false);
            var parent = parents.SingleOrDefault();

            return parent?.Id ?? Guid.Empty;
        }

        if (userRole is Role.Provider or Role.Employee)
        {
            var providerId = await officialRepository.GetProviderIdByOfficialUserIdAsync(userId);
            
            return providerId;
        }

        return Guid.Empty;
    }
}