
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
    private readonly OutOfSchoolDbContext _dbContext;

    public DirectorManagementService(
       IOfficialRepository officialRepository,
       IOfficialChangesLogService officialChangesLogService,
    IPositionRepository positionRepository,
       IPositionService positionService,
       ICurrentUserService currentUserService,
       ILogger<DirectorManagementService> logger,
       OutOfSchoolDbContext dbContext)
    {
        this._officialRepository = officialRepository;
        this._officialChangesLogService = officialChangesLogService;
        this._positionRepository = positionRepository;
        this._positionService = positionService;
        this._currentUserService = currentUserService;
        this._logger = logger;
        this._dbContext = dbContext;
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

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
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

                await transaction.CommitAsync();
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
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to promote employee to Director for provider {ProviderId}.", providerId);
                throw; 
            }
        });
    }
    
    public async Task<TransferDirectorResponseDto> TransferDirectorPosition(Guid providerId, TransferDirectorRequestDto request)
    {
        // 1. Завантаження об'єктів та валідація до транзакції
        var fromOfficial = await _officialRepository
            .Get()
            .Include(o => o.Position)
            .Include(o => o.Individual)
            .FirstOrDefaultAsync(o => o.Id == request.FromOfficialId);

        var toOfficial = await _officialRepository
            .Get()
            .Include(o => o.Position)
            .Include(o => o.Individual)
            .FirstOrDefaultAsync(o => o.Id == request.ToOfficialId);

        ValidateOfficialsExist(fromOfficial, toOfficial);
        ValidateSameProvider(fromOfficial, toOfficial, providerId);
        ValidateCurrentUserIsDirector(fromOfficial, _currentUserService.UserId);
        //await ValidateDirectorTransfer(providerId, request.FromOfficialId, request.ToOfficialId);

        // 2. Транзакція –  зміна PositionType
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            fromOfficial.Position.PositionType = PositionType.DeputyDirector; // ??? need this
            toOfficial.Position.PositionType = PositionType.Director;

            await _positionRepository.Update(fromOfficial.Position);
            await _positionRepository.Update(toOfficial.Position);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogDebug("Transferred director from {From} to {To} for provider {ProviderId}",
                request.FromOfficialId, request.ToOfficialId, providerId);

            return new TransferDirectorResponseDto
            {
                ProviderId = providerId,
                PreviousDirectorOfficialId = fromOfficial.Id,
                PreviousDirectorFullName = $"{fromOfficial.Individual.LastName} {fromOfficial.Individual.FirstName}",
                NewDirectorOfficialId = toOfficial.Id,
                NewDirectorFullName = $"{toOfficial.Individual.LastName} {toOfficial.Individual.FirstName}"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error transferring director role from {From} to {To}", request.FromOfficialId, request.ToOfficialId);
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
            throw new ArgumentException("One or both officials not found.");
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

    private async Task ValidateDirectorTransfer(Guid providerId, Guid fromOfficialId, Guid toOfficialId)
    {
        var currentUserId = _currentUserService.UserId;

        var fromOfficial = await _officialRepository.GetById(fromOfficialId);
        var toOfficial = await _officialRepository.GetById(toOfficialId);

        if (fromOfficial is null || toOfficial is null)
        {
            throw new ArgumentException("One or both officials do not exist.");
        }

        if (fromOfficial.Position.ProviderId != providerId || toOfficial.Position.ProviderId != providerId)
        {
            throw new InvalidOperationException("Officials must belong to the same provider.");
        }

        if (fromOfficial.Individual.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only the current director can initiate a transfer.");
        }

        if (fromOfficial.Position.PositionType != PositionType.Director)
        {
            throw new InvalidOperationException("Only a director can transfer the director position.");
        }
    }

    //private async Task ValidateActiveDirector2(Guid providerId, Guid officialId)
    //{
    //    var official = await _officialRepository.GetById(officialId);

    //    if (official == null)
    //        throw new Exception("Current director not found.");

    //    if (official.Position == null)
    //        throw new Exception("Current director has no assigned position.");

    //    if (official.Position.ProviderId != providerId)
    //        throw new Exception("Current director does not belong to the given provider.");

    //    if (official.Position.PositionType != PositionType.Director)
    //        throw new Exception("This official is not assigned to a director position.");

    //    var now = DateOnly.FromDateTime(DateTime.UtcNow);
    //    if (official.ActiveTo != default && official.ActiveTo <= now)
    //        throw new Exception("The director's position is no longer active.");

    //    // Якщо потрібно — перевірити, що це саме той, хто ініціює дію
    //   // var currentUserId = _currentUserService.UserId(); // ??
    //    //if (official.IndividualId != currentUserId)
    //     //   throw new Exception("Only the current director can initiate a transfer.");
    //}
}
