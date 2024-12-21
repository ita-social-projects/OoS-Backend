using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Position;
using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;
public class PositionService : IPositionService
{    
    private readonly IEntityRepository<Guid, Position> _entityRepositoryBase;
    private IProviderService _providerService;
    private readonly ILogger<Position> _logger;
    private readonly IMapper _mapper;    

    public PositionService(ILogger<Position> logger, IMapper mapper,
        IEntityRepository<Guid, Position> entityRepositoryBase,
        IProviderService providerService)
    {
        this._logger = logger;
        this._mapper = mapper;    
        this._entityRepositoryBase = entityRepositoryBase; 
        this._providerService = providerService;
    }

    public async Task<PositionDto> CreateAsync(PositionCreateDto createDto, Guid providerId)
    {
        // Check if the provider exists using the provider repository
        var provider = await _providerService.GetByUserId(providerId.ToString());
        
        if (provider == null)
        {
            _logger.LogError($"Position with ID {provider.Id} not found.");
            throw new ArgumentException($"Provider with ID {provider.Id} does not exist.");
        }

        try
        {            
            var position = _mapper.Map<Position>(createDto);
            position.ProviderId = provider.Id; // link position to provider id

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

    public async Task<IEnumerable<PositionDto>> GetAllAsync(Guid providerId)
    {                
        var positions = await _entityRepositoryBase.GetByFilter(x=> x.CreatedBy == providerId.ToString());        
        return positions.Select(p => _mapper.Map<PositionDto>(p));
    }

    public async Task<PositionDto> GetByIdAsync(Guid id)
    {
        var position = await _entityRepositoryBase.GetById(id);
        if (position == null)
        {
            _logger.LogError($"Position with ID {id} not found.");
            throw new KeyNotFoundException($"Position with ID {id} not found.");
        }
        return _mapper.Map<PositionDto>(position);
    }

    public async Task<PositionDto> UpdateAsync(Guid id, PositionUpdateDto updateDto, Guid providerId)
    {
        var existingPosition = await _entityRepositoryBase.GetById(id);
        if (existingPosition == null || existingPosition.CreatedBy != providerId.ToString())
        {
            throw new UnauthorizedAccessException("You do not have permission to update this position.");
        }

        _mapper.Map(updateDto, existingPosition);
        var updatedPosition = await _entityRepositoryBase.Update(existingPosition);
        return _mapper.Map<PositionDto>(updatedPosition);
    }

    public async Task DeleteAsync(Guid id, Guid providerOwnerId)
    {
        var position = await _entityRepositoryBase.GetById(id);

        if (position == null || position.IsDeleted)
        {
            _logger.LogError($"Position with ID {id} not found.");
            throw new KeyNotFoundException($"Position with ID {id} not found or it was deleted.");
        }

        if (position.CreatedBy != providerOwnerId.ToString())
        {
            _logger.LogError($"Unauthorized deletion attempt for position {id} by provider id {providerOwnerId}");
            throw new UnauthorizedAccessException("You do not have permission to delete this position.");
        }

        _logger.LogInformation($"Deleting position {id} for provider {providerOwnerId}");
        await _entityRepositoryBase.Delete(position);
    }   
}
