using AutoMapper;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.BusinessLogic.Models.Position;
using Newtonsoft.Json;

namespace OutOfSchool.BusinessLogic.Services;
public class PositionService : IPositionService
{
    private readonly IPositionRepository _positionRepository;  
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<SocialGroupService> _logger;
    private readonly IMapper _mapper;    

    public PositionService(ILogger<SocialGroupService> logger, IMapper mapper, 
        IPositionRepository positionRepository,
        IProviderRepository providerRepository)
    {
        this._logger = logger;
        this._mapper = mapper;    
        this._positionRepository = positionRepository;        
        this._providerRepository = providerRepository;
    }

    public async Task<PositionDto> CreateAsync(PositionCreateDto createDto, Guid providerId)
    {
        // Check if the provider exists using the provider repository
        var provider = await _providerRepository.GetProviderByUserIdAsync(providerId);

        var position = _mapper.Map<Position>(createDto);       
        position.ProviderId = provider.Id; // Linking provider userId to position
                
        try
        {
            _logger.LogInformation($"Creating position: {JsonConvert.SerializeObject(position)}");

            var createdPosition = await _positionRepository.CreateAsync(position);
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
        var positions = await _positionRepository.GetAllAsync(providerId);        
        return positions.Select(p => _mapper.Map<PositionDto>(p));
    }

    public async Task<PositionDto> GetByIdAsync(Guid id)
    {
        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null)
        {
            _logger.LogError($"Position with ID {id} not found.");
            throw new KeyNotFoundException($"Position with ID {id} not found.");
        }
        return _mapper.Map<PositionDto>(position);
    }

    public async Task<PositionDto> UpdateAsync(Guid id, PositionUpdateDto updateDto, Guid providerId)
    {
        var existingPosition = await _positionRepository.GetByIdAsync(id);
        if (existingPosition == null || existingPosition.CreatedBy != providerId.ToString())
        {
            throw new UnauthorizedAccessException("You do not have permission to update this position.");
        }

        _mapper.Map(updateDto, existingPosition);
        var updatedPosition = await _positionRepository.UpdateAsync(existingPosition);
        return _mapper.Map<PositionDto>(updatedPosition);
    }

    public async Task DeleteAsync(Guid id, Guid providerId)
    {
        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null || position.ProviderId != providerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this position.");
        }

        await _positionRepository.DeleteAsync(position);
    }   
}
