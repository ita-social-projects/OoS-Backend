using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;
public class DirectorManagementService : IDirectorManagementService
{
    private readonly IOfficialRepository _officialRepository;
    private readonly IOfficialChangesLogService _officialChangesLogService;
    private readonly IPositionRepository _positionRepository;
    private readonly IPositionService _positionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DirectorManagementService> _logger;
    private readonly ITransactionManagerService _transactionManagerService;

    /// <summary>
    /// Create a delegate to include other entities in Official entity
    /// </summary>
    private readonly Func<IQueryable<Official>, IQueryable<Official>> _includeOfficialFunc =
    query => query
        .Include(o => o.Position)
        .Include(o => o.Individual);

    public DirectorManagementService(
       IOfficialRepository officialRepository,
       IOfficialChangesLogService officialChangesLogService,
       IPositionRepository positionRepository,
       IPositionService positionService,
       ICurrentUserService currentUserService,
       ITransactionManagerService transactionManagerService,
       ILogger<DirectorManagementService> logger)
    {
        this._officialRepository = officialRepository;
        this._officialChangesLogService = officialChangesLogService;
        this._positionRepository = positionRepository;
        this._positionService = positionService;
        this._currentUserService = currentUserService;
        this._transactionManagerService = transactionManagerService;
        this._logger = logger;
    }

    public async Task<Result<PromoteToDirectorResponseDto>> PromoteEmployeeToDirector(Guid providerId, PromoteToDirectorRequestDto request)
    {
        // check if the current user is a deputy director of the provider
        await _currentUserService.UserHasRights(new DeputyDirectorRights(providerId));

        if (await _positionRepository.DirectorExistsAsync(providerId))
        {
            _logger.LogWarning("Attempted to promote new director for provider {ProviderId}, but one already exists.", providerId);
            return Result<PromoteToDirectorResponseDto>.Failed(new OperationError
            {
                Code = "DirectorAlreadyExists",
                Description = $"Director already exists for provider with ID: {providerId}"
            });
        }
        // Check if the current user is a deputy director of the provider
        var userId = _currentUserService.UserId;
        var initiator = await _officialRepository.GetByUserIdAsync(userId);
        if (initiator?.Position?.ProviderId != providerId ||
            (initiator.Position.PositionType != PositionType.DeputyDirector))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized promotion. Not Deputy or wrong provider.", userId);
            return Result<PromoteToDirectorResponseDto>.Failed(new OperationError
            {
                Code = "Unauthorized",
                Description = "Only Director or Deputy of this provider can promote."
            });
        }

        // Get the official to be promoted
        var official = await _officialRepository.GetById(request.OfficialId);
        if (official == null)
        {
            _logger.LogWarning("Promotion failed: official with ID {OfficialId} not found.", request.OfficialId);
            return Result<PromoteToDirectorResponseDto>.Failed(new OperationError
            {
                Code = "OfficialNotFound",
                Description = $"Official with ID {request.OfficialId} not found"
            });
        }
        if (official.Position?.ProviderId != providerId)
        {
            _logger.LogWarning("Official {OfficialId} does not belong to provider {ProviderId}.", official.Id, providerId);
            return Result<PromoteToDirectorResponseDto>.Failed(new OperationError
            {
                Code = "WrongProvider",
                Description = "Official does not belong to the specified provider."
            });
        }


        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldPositionType = official.Position.PositionType; // Save the old position type for logging

        try
        {
            var result = await _transactionManagerService.ExecuteInTransactionAsync(async () =>
            {
                // Close the old position
                official.Position.ActiveTo = now;
                await _positionRepository.Update(official.Position);

                // 2. Create a new position for the director
                var createDto = official.Position.ToDirectorCreateDto();
                var newDirectorPosition = await _positionService.CreateAsync(createDto, providerId);

                // Connect new position to the official
                official.PositionId = newDirectorPosition.Id;
                await _officialRepository.Update(official);

                await _officialChangesLogService.SaveChangesLogAsync(
                  official,
                  _currentUserService.UserId,
                  OperationType.PromotedToDirector,
                  nameof(Position.PositionType),
                  oldPositionType.ToString(),
                  PositionType.Director.ToString());

                _logger.LogInformation("Official {OfficialId} has been promoted to Director for provider {ProviderId}. New PositionId: {PositionId}",
                            official.Id, providerId, newDirectorPosition.Id);

                return official.ToPromoteDto(newDirectorPosition);
            });
            return Result<PromoteToDirectorResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to promote employee to Director. ProviderId: {ProviderId}, OfficialId: {OfficialId}", providerId, request.OfficialId);
            return Result<PromoteToDirectorResponseDto>.Failed(new OperationError
            {
                Code = "PromotionFailed",
                Description = "An unexpected error occurred during the promotion process."
            });
        }
    }

    public async Task<Result<TransferDirectorResponseDto>> TransferDirectorPosition(Guid providerId, TransferDirectorRequestDto request)
    {
        await _currentUserService.UserHasRights(new ProviderRights(providerId));

        try
        {
            return await _officialRepository.RunInTransaction(async () =>
            {
                var fromOfficial = await _officialRepository.GetByIdWithDetails(
                    request.FromOfficialId,
                    includeExpression: _includeOfficialFunc);
                var toOfficial = await _officialRepository.GetByIdWithDetails(
                    request.ToOfficialId,
                    includeExpression: _includeOfficialFunc);

                var validationResult = ValidateOfficialsExist(fromOfficial, toOfficial);
                if (!validationResult.Succeeded)
                {
                    _logger.LogWarning("Validation failed: {Error}", validationResult.ToString());
                    return Result<TransferDirectorResponseDto>.Failed(validationResult.Errors.ToArray());
                }

                validationResult = ValidateSameProvider(fromOfficial, toOfficial, providerId);
                if (!validationResult.Succeeded)
                {
                    _logger.LogWarning("Validation failed: {Error}", validationResult.ToString());
                    return Result<TransferDirectorResponseDto>.Failed(validationResult.Errors.ToArray());
                }

                validationResult = ValidateCurrentUserIsDirector(fromOfficial, _currentUserService.UserId);
                if (!validationResult.Succeeded)
                {
                    _logger.LogWarning("Validation failed: {Error}", validationResult.ToString());
                    return Result<TransferDirectorResponseDto>.Failed(validationResult.Errors.ToArray());
                }

                var now = DateOnly.FromDateTime(DateTime.UtcNow);

                // swap positions between actual director and other employee
                var tmp = toOfficial.PositionId;
                toOfficial.PositionId = fromOfficial.PositionId;
                fromOfficial.PositionId = tmp;

                var tempPosition = toOfficial.Position;
                toOfficial.Position = fromOfficial.Position;
                fromOfficial.Position = tempPosition;

                fromOfficial.ActiveFrom = now;
                toOfficial.ActiveFrom = now;

                await _officialRepository.Update(fromOfficial);
                await _officialRepository.Update(toOfficial);

                await _officialChangesLogService.SaveChangesLogAsync(
                    fromOfficial,
                    _currentUserService.UserId,
                    OperationType.TransferredToDirector,
                    nameof(Position.PositionType),
                    PositionType.Director.ToString(),
                    fromOfficial.Position.PositionType.ToString());

                await _officialChangesLogService.SaveChangesLogAsync(
                    toOfficial,
                    _currentUserService.UserId,
                    OperationType.TransferredToDirector,
                    nameof(Position.PositionType),
                    fromOfficial.Position.PositionType.ToString(),
                    PositionType.Director.ToString());

                _logger.LogInformation("Transferred director from {From} to {To} for provider {ProviderId}",
                    request.FromOfficialId, request.ToOfficialId, providerId);

                var dto = fromOfficial.ToTransferDirectorDto(toOfficial, providerId);
                return Result<TransferDirectorResponseDto>.Success(dto);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during director transfer. ProviderId: {ProviderId}, FromOfficialId: {FromOfficialId}",
                providerId, request.FromOfficialId);
            return Result<TransferDirectorResponseDto>.Failed(new OperationError
            {
                Code = "TransferFailed",
                Description = "An unexpected error occurred during the transfer process."
            });
        }
    }

    private static OperationResult ValidateOfficialsExist(Official from, Official to)
    {
        if (from == null || to == null)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = "OfficialsNotFound",
                Description = "One or both officials were not found."
            });
        }

        return OperationResult.Success;
    }

    private static OperationResult ValidateSameProvider(Official from, Official to, Guid providerId)
    {
        if (from.Position.ProviderId != providerId || to.Position.ProviderId != providerId)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = "DifferentProvider",
                Description = "Officials must belong to the same provider."
            });
        }

        return OperationResult.Success;
    }

    private static OperationResult ValidateCurrentUserIsDirector(Official from, string currentUserId)
    {
        if (from.Individual.UserId != currentUserId)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = "Unauthorized",
                Description = "Only the current director can initiate a transfer."
            });
        }

        if (from.Position.PositionType != PositionType.Director)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = "NotDirector",
                Description = "Only a director can transfer the director position."
            });
        }

        return OperationResult.Success;
    }
}
