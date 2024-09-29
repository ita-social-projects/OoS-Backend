using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

public interface IValidationService
{
    /// <summary>
    /// Check if Provider with specified providerId has the same userId.
    /// </summary>
    /// <param name="userId">Id of User.</param>
    /// <param name="providerId">Id of Provider.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="bool"/>: true if the User is owner of the specified providerId, false if not or Provider with specified providerId was not found.</returns>
    /// <exception cref="InvalidOperationException">If the logic of creating providers was compromised.</exception>
    Task<bool> UserIsProviderOwnerAsync(string userId, Guid providerId);

    /// <summary>
    /// Check if Workshop's Provider with specified workshopId has the same userId.
    /// </summary>
    /// <param name="userId">Id of User.</param>
    /// <param name="workshopId">Id of Workshop.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="bool"/>: true if the User is owner of the specified workshopId, false if not or Workshop with specified workshopId was not found.</returns>
    /// <exception cref="InvalidOperationException">If the logic of creating providers was compromised.</exception>
    Task<bool> UserIsWorkshopOwnerAsync(string userId, Guid workshopId);

    /// <summary>
    /// Check if Parent with specified parentId has the same userId.
    /// </summary>
    /// <param name="userId">Id of User.</param>
    /// <param name="parentId">Id of Parent.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="bool"/>: true if the User is owner of the specified parentId, false if not or Parent with specified parentId was not found.</returns>
    /// <exception cref="InvalidOperationException">If the logic of creating providers was compromised.</exception>
    Task<bool> UserIsParentOwnerAsync(string userId, Guid parentId);

    /// <summary>
    /// Get the provider's or parent's Id according to the user role.
    /// </summary>
    /// <param name="userId">Id of User.</param>
    /// <param name="userRole">The role of the user.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains the Id (type of <see cref="Guid"/>). If Provider or Parent with specified userId was not found, the result will be zero.</returns>
    /// <exception cref="InvalidOperationException">If the logic of creating providers was compromised.</exception>
    Task<Guid> GetParentOrProviderIdByUserRoleAsync(string userId, Role userRole);
}