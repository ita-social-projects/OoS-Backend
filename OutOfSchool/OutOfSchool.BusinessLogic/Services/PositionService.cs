using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Repository.Base.Api;
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
    }

    public async Task<PositionDto> CreateAsync(PositionCreateUpdateDto createDto, Guid providerId)
    {
        // Check if the provider has rights to create a new position        
        await _providerService.HasProviderRights(providerId);

        // Get provider by current user id to use it`s data in a new position
        var provider = await _providerService.GetByUserId(_currentUserService.UserId.ToString());

        var position = _mapper.Map<Position>(createDto);
        position.ProviderId = provider.Id; // link position to provider positionId
        
        var createdPosition = await _entityRepositoryBase.Create(position);
        _logger.LogDebug($"Created position with id: {position.Id}");
        return _mapper.Map<PositionDto>(createdPosition);
    }

    public async Task<SearchResult<PositionDto>> GetByFilter(Guid providerId, PositionsFilter filter)
    {
        await _providerService.HasProviderRights(providerId);        

        _logger.LogInformation("Getting all Positions started (by filter)");

        filter ??= new PositionsFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var predicate = PredicateBuilder.True<Position>();

        // Filter by FullName
        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            predicate = predicate.And(p => p.FullName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));
        }

        // Filter out deleted positions and take only positions for given provider
        predicate = predicate.And(p => !p.IsDeleted)
                             .And(p => p.ProviderId == providerId);

        // Define sorting
        var sortPredicate = SortExpressionBuild(filter);
        
        int count = await _entityRepositoryBase.Count(predicate).ConfigureAwait(false);
       
        var positions = await _entityRepositoryBase
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate,
                orderBy: sortPredicate)
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
        await _providerService.HasProviderRights(providerId);
        var position = await GetPositionAsync(positionId, providerId);
        
        return _mapper.Map<PositionDto>(position);
    }

    public async Task<PositionDto> UpdateAsync(Guid positionId, PositionCreateUpdateDto updateDto, Guid providerId)
    {
        await _providerService.HasProviderRights(providerId);
        var existingPosition = await GetPositionAsync(positionId, providerId);
        _mapper.Map(updateDto, existingPosition);
        
        var updatedPosition = await _entityRepositoryBase.Update(existingPosition);
        
        return _mapper.Map<PositionDto>(updatedPosition);
    }

    public async Task DeleteAsync(Guid positionId, Guid providerId)
    {
        await _providerService.HasProviderRights(providerId);

        var position = await GetPositionAsync(positionId, providerId);

        _logger.LogInformation($"Deleting position {positionId} for provider {providerId}");
        await _entityRepositoryBase.Delete(position);
    }

    private async Task CheckIfExist(Guid positionId, Guid providerId)
    {
        var position = await _entityRepositoryBase.GetByFilter(
                x => x.Id == positionId && x.ProviderId == providerId && !x.IsDeleted).ConfigureAwait(false);

        if (position.Count() == 0 || position.Single().IsDeleted == true)
        {
            _logger.LogError($"Position with positionId {positionId} not found.");
            throw new KeyNotFoundException($"Position with positionId {positionId} not found or it was deleted.");
        }                
    }

    public async Task<Position> GetPositionAsync(Guid positionId, Guid providerId)
    {
        await CheckIfExist(positionId, providerId);

        var position = await _entityRepositoryBase.GetByFilter(
            x => x.Id == positionId && x.ProviderId == providerId && !x.IsDeleted)
            .ConfigureAwait(false);

        return position.SingleOrDefault();
    }

    private static Dictionary<Expression<Func<Position, object>>, SortDirection> SortExpressionBuild(
        PositionsFilter filter)
    {
        var sortExpression = new Dictionary<Expression<Func<Position, object>>, SortDirection>();
        
        sortExpression.Add(
           a => a.FullName,
           filter.OrderByFullName ? SortDirection.Ascending : SortDirection.Descending
       );

        sortExpression.Add(a => a.CreatedAt, filter.OrderByCreatedAt ?
            SortDirection.Ascending :
            SortDirection.Descending);

        return sortExpression;
    }
}
