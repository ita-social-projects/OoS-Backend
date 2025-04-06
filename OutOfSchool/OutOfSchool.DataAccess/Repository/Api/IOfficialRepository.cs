using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

/// <summary>
/// Represents a repository for managing Official entities with soft delete functionality.
/// </summary>
public interface IOfficialRepository: ISensitiveEntityRepositorySoftDeleted<Official>
{
    /// <summary>
    /// Retrieves the provider ID associated with an employee's user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the provider's unique identifier.</returns>
    Task<Guid> GetProviderIdByOfficialUserIdAsync(string userId);
    
    /// <summary>
    /// Retrieves a list of user IDs for all active officials associated with a specific provider.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of user IDs for active officials.</returns>
    Task<List<string>> GetActiveOfficialUserIdsByProviderId(Guid providerId);
    
    /// <summary>
    /// Retrieves the user ID of the director official associated with a specific provider.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user ID of the director official.</returns>
    Task<string> GetDirectorOfficialUserIdByProviderIdAsync(Guid providerId);
}