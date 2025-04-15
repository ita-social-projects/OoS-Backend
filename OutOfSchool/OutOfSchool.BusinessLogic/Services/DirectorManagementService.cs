
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;
public class DirectorManagementService : IDirectorManagementService
{
    private readonly IOfficialRepository _officialRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IPositionService _positionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DirectorManagementService> _logger;
    private readonly OutOfSchoolDbContext _dbContext;

    public DirectorManagementService(
       IOfficialRepository officialRepository,
       IPositionRepository positionRepository,
       IPositionService positionService,
       ICurrentUserService currentUserService,
       ILogger<DirectorManagementService> logger,
       OutOfSchoolDbContext dbContext)
    {
        this._officialRepository = officialRepository;
        this._positionRepository = positionRepository;
        this._positionService = positionService;
        this._currentUserService = currentUserService;
        this._logger = logger;
        this._dbContext = dbContext;
    }

    public async Task<PromoteToDirectorResponseDto> PromoteEmployeeToDirector(Guid providerId, PromoteToDirectorRequestDto request)
    {
        // check if the current user is a deputy director of the provider
        await _currentUserService.UserHasRights(new ProviderRights(providerId));

        if (await _positionRepository.DirectorExistsAsync(providerId))
        {
            _logger.LogWarning("Attempted to promote new director for provider {ProviderId}, but one already exists.", providerId);
            throw new InvalidOperationException($"Director already exists for provider with ID: {providerId}");
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

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Close the old position
            official.Position.ActiveTo = now;
            await _positionRepository.Update(official.Position);

            // 2. Create a new position for the director
            var createDto = new PositionCreateUpdateDto
            {
                FullName = official.Position.FullName,
                ShortName = official.Position.ShortName,
                GenitiveName = official.Position.GenitiveName,
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

            // Deactivate the deputy director position (for the initiator)
            var userId = Guid.Parse(_currentUserService.UserId);
            var initiator = await _officialRepository.GetById(userId);
            if (initiator?.Position?.PositionType == PositionType.DeputyDirector &&
                initiator.Position.ProviderId == providerId)
            {
                initiator.Position.ActiveTo = now;
                await _positionRepository.Update(initiator.Position);
                _logger.LogInformation("Initiator (ID: {InitiatorId}) was a deputy and had their position deactivated.", userId);
            }
            await transaction.CommitAsync();
            _logger.LogInformation("Official {OfficialId} has been promoted to Director for provider {ProviderId}. New PositionId: {PositionId}",
                    official.Id, providerId, newDirectorPosition.Id);
            return new PromoteToDirectorResponseDto
            {
                OfficialId = official.Id,
                PositionId = newDirectorPosition.Id,
                ActiveFrom = newDirectorPosition.ActiveFrom,
                PositionType = newDirectorPosition.PositionType,
                FullName = "Директор"
            };
        });
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
