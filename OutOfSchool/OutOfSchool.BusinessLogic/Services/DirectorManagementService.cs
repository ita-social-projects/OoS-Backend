
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
                var createDto = new PositionCreateUpdateDto
                {
                    FullName = "Директор ЗО",
                    ShortName = "Директор",
                    GenitiveName = "Директору",
                    Rate = official.Position.Rate,
                    Tariff = official.Position.Tariff,
                    ClassifierType = official.Position.ClassifierType,
                    Department = official.Position.Department,
                    IsForRuralAreas = official.Position.IsForRuralAreas,
                    IsTeachingPosition = false,
                    PositionType = PositionType.Director,
                };
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
                return new PromoteToDirectorResponseDto
                {
                    OfficialId = official.Id,
                    PositionId = newDirectorPosition.Id,
                    ActiveFrom = newDirectorPosition.ActiveFrom,
                    PositionType = newDirectorPosition.PositionType,
                    FullName = newDirectorPosition.FullName,
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to promote employee to Director. ProviderId: {ProviderId}, OfficialId: {OfficialId}", providerId, request.OfficialId);
            throw;
        }
    }

    public Task TransferDirectorPosition(Guid providerId, TransferDirectorRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateDirectorshipRequirements(Guid providerId)
    {
        throw new NotImplementedException();
    }
}
