using OutOfSchool.BusinessLogic.Models.Official;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for managing director positions within a provider.
/// Includes functionality to promote employees and transfer director role.
/// </summary>
public interface IDirectorManagementService
{
    /// <summary>
    /// Promotes an employee (official) to the director position for the given provider.
    /// Allowed only if no active director exists.
    /// </summary>
    /// <param name="providerId">ID of the provider for which the promotion is requested.</param>
    /// <param name="request">DTO containing the ID of the official to promote.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<PromoteToDirectorResponseDto> PromoteEmployeeToDirector(Guid providerId, PromoteToDirectorRequestDto request);

    /// <summary>
    /// <summary>
    /// Transfers the director position from the current director to another employee within the same provider.
    /// The operation is allowed only if the current user is the existing director (initiator).
    /// </summary>
    /// <param name="providerId">The ID of the provider where the transfer is being performed. Extracted from the route.</param>
    /// <param name="request">A DTO containing the IDs of the current director and the employee who will become the new director.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<TransferDirectorResponseDto> TransferDirectorPosition(Guid providerId, TransferDirectorRequestDto request);
}
