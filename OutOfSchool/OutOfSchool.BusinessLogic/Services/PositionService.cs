using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Position;
using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Common.Models;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Services;
public class PositionService : IPositionService
{    
    private readonly IEntityRepository<Guid, Position> _entityRepositoryBase;
    private IProviderService _providerService;
    private readonly ILogger<Position> _logger;
    private readonly IMapper _mapper;    
    private readonly ICurrentUserService _currentUserService;
    private readonly Guid currentUserId;

    public PositionService(ILogger<Position> logger, IMapper mapper,
                           IEntityRepository<Guid, Position> entityRepositoryBase,
                           IProviderService providerService,
                           ICurrentUserService currentUserService)
    {
        this._logger = logger;
        this._mapper = mapper;    
        this._entityRepositoryBase = entityRepositoryBase; 
        this._providerService = providerService;
        this._currentUserService = currentUserService;

        currentUserId = GetCurrentUserId();
    }

    public async Task<PositionDto> CreateAsync(PositionCreateUpdateDto createDto, Guid providerId)
    {
        // Check if the provider has rights to create a new position
        await _currentUserService.UserHasRights(new ProviderRights(providerId));

        // Get provider to use it`s data in a new position
        var provider = await _providerService.GetByUserId(currentUserId.ToString());                

        try
        {            
            var position = _mapper.Map<Position>(createDto);
            position.ProviderId = provider.Id; // link position to provider positionId

            _logger.LogInformation($"Creating position: {JsonConvert.SerializeObject(position)}");

            var createdPosition = await _entityRepositoryBase.Create(position);
            return _mapper.Map<PositionDto>(createdPosition);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Can`t create a new object: {ex}");
        }
        
        return null;
    }

    public async Task<SearchResult<PositionDto>> GetByFilter(Guid providerId, PositionsFilter filter)
    {
        await _currentUserService.UserHasRights(new ProviderRights(providerId));

        _logger.LogInformation("Getting all Positions started (by filter)");

        filter ??= new PositionsFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var predicate = PredicateBuilder.True<Position>();

        // Filter by FullName
        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            predicate = predicate.And(p => p.FullName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));
        }

        // Filter out deleted positions
        predicate = predicate.And(p => !p.IsDeleted);

        int count = await _entityRepositoryBase.Count(predicate).ConfigureAwait(false);

        // Define sorting
        var orderBy = new Dictionary<Expression<Func<Position, object>>, SortDirection>
    {
        { p => p.FullName, SortDirection.Ascending },
        { p => p.CreatedAt, SortDirection.Ascending }
    };

        var positions = await _entityRepositoryBase
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate,
                orderBy: orderBy)
            .ToListAsync()
            .ConfigureAwait(false);

        _logger.LogInformation(!positions.Any()
            ? "No positions found matching the filter criteria."
            : $"Retrieved {positions.Count} positions.");

        var positionsDto = positions.Select(position => _mapper.Map<PositionDto>(position)).ToList();

        var result = new SearchResult<PositionDto>
        {
            TotalAmount = count,
            Entities = positionsDto,
        };

        return result;
    }


    public async Task<PositionDto> GetByIdAsync(Guid positionId, Guid providerId)
    {
        await _currentUserService.UserHasRights(new ProviderRights(providerId));

        var position = await _entityRepositoryBase.GetById(positionId);
        if (position == null)
        {
            _logger.LogError($"Position with positionId {positionId} not found.");
            throw new KeyNotFoundException($"Position with positionId {positionId} not found.");
        }
        return _mapper.Map<PositionDto>(position);
    }

    public async Task<PositionDto> UpdateAsync(Guid positionId, PositionCreateUpdateDto updateDto, Guid providerId)
    {
        await _currentUserService.UserHasRights(new ProviderRights(providerId));

        var existingPosition = await _entityRepositoryBase.GetById(positionId);
                        
        _mapper.Map(updateDto, existingPosition);
        var updatedPosition = await _entityRepositoryBase.Update(existingPosition);
        return _mapper.Map<PositionDto>(updatedPosition);
    }

    public async Task DeleteAsync(Guid positionId, Guid providerId)
    {
        await _currentUserService.UserHasRights(new ProviderRights(providerId));
        
        var position = await _entityRepositoryBase.GetById(positionId);

        if (position == null || position.IsDeleted == true)
        {
            _logger.LogError($"Position with positionId {positionId} not found.");
            throw new KeyNotFoundException($"Position with positionId {positionId} not found or it was deleted.");
        }        

        _logger.LogInformation($"Deleting position {positionId} for provider {currentUserId}");
        await _entityRepositoryBase.Delete(position);
    }

    // Method to current user positionId
    private Guid GetCurrentUserId()
    {              
        var positionId = _currentUserService.UserId;
        return Guid.Parse(positionId);                
    }
}
