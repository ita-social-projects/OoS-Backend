
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

    public async Task<PromoteToDirectorResponseDto> PromoteEmployeeToDirector(Guid providerId, PromoteToDirectorRequestDto request)
    {
        // check if the current user is a deputy director of the provider
        await _currentUserService.UserHasRights(new DeputyDirectorRights(providerId));

        if (await _positionRepository.DirectorExistsAsync(providerId))
        {
            _logger.LogWarning("Attempted to promote new director for provider {ProviderId}, but one already exists.", providerId);
            throw new InvalidOperationException($"Director already exists for provider with ID: {providerId}");
        }
        // Check if the current user is a deputy director of the provider
        var userId = _currentUserService.UserId;
        var initiator = await _officialRepository.GetByUserIdAsync(userId);
        if (initiator?.Position?.ProviderId != providerId ||
            (initiator.Position.PositionType != PositionType.DeputyDirector))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized promotion. Not Deputy or wrong provider.", userId);
            throw new UnauthorizedAccessException("Only Director or Deputy of this provider can promote.");
        }

        // Get the official to be promoted
        var official = await _officialRepository.GetById(request.OfficialId);
        if (official == null)
        {
            _logger.LogWarning("Promotion failed: official with ID {OfficialId} not found.", request.OfficialId);
            throw new KeyNotFoundException($"Official with ID {request.OfficialId} not found");
        }
        if (official.Position?.ProviderId != providerId)
        {
            _logger.LogWarning("Official {OfficialId} does not belong to provider {ProviderId}.", official.Id, providerId);
            throw new InvalidOperationException("Official does not belong to the specified provider.");
        }


        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldPositionType = official.Position.PositionType; // Save the old position type for logging

        try
        {
            return await _transactionManagerService.ExecuteInTransactionAsync(async () =>
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to promote employee to Director. ProviderId: {ProviderId}, OfficialId: {OfficialId}", providerId, request.OfficialId);
            throw;
        }
    }
    
    public async Task<TransferDirectorResponseDto> TransferDirectorPosition(Guid providerId, TransferDirectorRequestDto request)
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

                ValidateOfficialsExist(fromOfficial, toOfficial);
                ValidateSameProvider(fromOfficial, toOfficial, providerId);
                ValidateCurrentUserIsDirector(fromOfficial, _currentUserService.UserId);

                var now = DateOnly.FromDateTime(DateTime.UtcNow);

                // swap positions between actual director and other employee
                var tmp = toOfficial.PositionId;
                toOfficial.PositionId = fromOfficial.PositionId;
                fromOfficial.PositionId = tmp;

                // need this?
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
                    OperationType.TranserredToDirector,
                    nameof(Position.PositionType),
                    PositionType.Director.ToString(),
                    fromOfficial.Position.PositionType.ToString());

                await _officialChangesLogService.SaveChangesLogAsync(
                    toOfficial,
                    _currentUserService.UserId,
                    OperationType.TranserredToDirector,
                    nameof(Position.PositionType),
                    fromOfficial.Position.PositionType.ToString(),
                    PositionType.Director.ToString());

                _logger.LogDebug("Transferred director from {From} to {To} for provider {ProviderId}",
                 request.FromOfficialId, request.ToOfficialId, providerId);

                return new TransferDirectorResponseDto
                {
                    ProviderId = providerId,
                    PreviousDirectorIndividualId = fromOfficial.IndividualId,
                    PreviousDirectorFullName = $"{fromOfficial.Individual.LastName} {fromOfficial.Individual.FirstName}",
                    NewDirectorIndividualId = toOfficial.IndividualId,
                    NewDirectorFullName = $"{toOfficial.Individual.LastName} {toOfficial.Individual.FirstName}"
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transfer Director position. ProviderId: {ProviderId}, OfficialId: {OfficialId}", providerId, request.FromOfficialId);
            throw;
        }
    }


    public Task<bool> ValidateDirectorshipRequirements(Guid providerId)
    {
        throw new NotImplementedException();
    }
    private void ValidateOfficialsExist(Official from, Official to)
    {
        if (from == null || to == null)
        {
            throw new KeyNotFoundException("One or both officials not found.");
        }
    }

    private void ValidateSameProvider(Official from, Official to, Guid providerId)
    {
        if (from.Position.ProviderId != providerId || to.Position.ProviderId != providerId)
        {
            throw new InvalidOperationException("Officials must belong to the same provider.");
        }
    }

    private void ValidateCurrentUserIsDirector(Official from, string currentUserId)
    {
        if (from.Individual.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only the current director can initiate a transfer.");
        }

        if (from.Position.PositionType != PositionType.Director)
        {
            throw new InvalidOperationException("Only a director can transfer the director position.");
        }
    }
}
