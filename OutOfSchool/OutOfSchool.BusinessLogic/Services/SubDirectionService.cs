using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;
/// <summary>
/// Initializes a new instance of the <see cref="SubDirectionService"/> class.
/// </summary>
/// <param name="subDirectionRepository">Repository for subdirections.</param>
/// <param name="directionRepository">Repository for directions.</param>
/// <param name="logger">Logger.</param>
/// <param name="mapper">Mapper.</param>
public class SubDirectionService(
    IEntityRepositorySoftDeleted<long, SubDirection> subDirectionRepository,
    IEntityRepositorySoftDeleted<long, Direction> directionRepository,
    ILogger<SubDirectionService> logger
) : ISubDirectionService, ISensitiveSubDirectionService
{
    /// <inheritdoc/>
    public async Task<SearchResult<SubDirectionDto>> GetByFilter(long directionId, SearchStringFilter filter)
    {
        logger.LogDebug("Getting SubDirections by filter started.");

        filter ??= new SearchStringFilter();
        var predicate = PredicateBuilder.True<SubDirection>();
        
        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            predicate = predicate.And(s => s.Title.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase)
                || s.Description.Contains(filter.SearchString, StringComparison.InvariantCultureIgnoreCase));
        }

        predicate = predicate.And(s => s.DirectionId == directionId);
        predicate = predicate.And(s => !s.IsDeleted);

        var sortExpression = new Dictionary<Expression<Func<SubDirection, object>>, SortDirection>
        {
            { x => x.Title, SortDirection.Ascending },
        };

        var count = await subDirectionRepository.Count(predicate).ConfigureAwait(false);

        var subDirections =await subDirectionRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                orderBy: sortExpression,
                whereExpression: predicate
            ).AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug("{Count} SubDirections were found.", count);

        var result = new SearchResult<SubDirectionDto>
        {
            Entities = subDirections.ToDto(),
            TotalAmount = count
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<SubDirectionDto> GetById(long id)
    {
        logger.LogDebug("Getting SubDirections by Id={Id} started.", id);

        var subDirection = await subDirectionRepository
            .GetById(id)
            .ConfigureAwait(false);

        if (subDirection == null)
        {
            logger.LogError("SubDirection with Id = {id} doesn't exist in the system.", id);
            return null;
        }

        logger.LogDebug("Successfully got a SubDirection with Id = {id}.", id);

        return subDirection.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<SubDirectionDto>> Update(SubDirectionDto dto)
    {
        logger.LogDebug("Updating SubDirection with Id = {id} started.", dto?.Id);

        if (dto == null)
        {
            logger.LogError("Updating failed. Dto is null");
            return Result<SubDirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Dto is null.",
            });
        }

        var subDirection = await subDirectionRepository.GetById(dto.Id).ConfigureAwait(false);

        if (subDirection is null)
        {
            logger.LogError("Updating failed. Direction with Id = {id} doesn't exist in the system.", dto.Id);
            return Result<SubDirectionDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"SubDirection with Id = {dto.Id} does not exist.",
            });
        }

        subDirection = await subDirectionRepository
            .Update(dto.SetToModel(subDirection, DateTime.UtcNow))
            .ConfigureAwait(false);

        logger.LogDebug("SubDirection with Id = {id} updated succesfully.", subDirection?.Id);

        return Result<SubDirectionDto>.Success(subDirection.ToDto());
    }

    /// <inheritdoc/>
    public async Task<Result<SubDirectionDto>> Create(long directionId, SubDirectionDto dto)
    {
        logger.LogDebug("SubDirection creation was started.");

        if (dto == null)
        {
            logger.LogError("Creating failed. Dto is null");
            return Result<SubDirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Dto is null.",
            });
        }

        var direction = await directionRepository.GetById(directionId).ConfigureAwait(false);

        if (direction == null)
        {
            logger.LogError("Creating failed. Direction with Id = {id} doesn't exist in the system.", directionId);
            return Result<SubDirectionDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"Direction with Id = {directionId} does not exist.",
            });
        }

        var validationErrors = await SubDirectionValidation(directionId, dto).ConfigureAwait(false);
        if (validationErrors.Any())
        {
            return Result<SubDirectionDto>.Failed(validationErrors.ToArray());
        }

        var subDirection = dto.ToModel(directionId);

        var newSubDirection = await subDirectionRepository.Create(subDirection).ConfigureAwait(false);

        logger.LogDebug("SubDirection with Id = {id} created successfully.", newSubDirection?.Id);

        return Result<SubDirectionDto>.Success(newSubDirection.ToDto());
    }

    /// <inheritdoc/>
    public async Task<Result<SubDirectionDto>> Delete(long id)
    {
        logger.LogDebug("Deleting SubDirection with Id = {id} started.", id);

        var subDirection = await subDirectionRepository.GetById(id).ConfigureAwait(false);

        if (subDirection == null)
        {
            return Result<SubDirectionDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"SubDirection with Id = {id} does not exist.",
            });
        }

        try
        {
            await subDirectionRepository.Delete(subDirection).ConfigureAwait(false);

            logger.LogDebug("SubDirection with Id = {id} succesfully deleted.", id);

            return Result<SubDirectionDto>.Success(subDirection.ToDto());
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Deleting failed. SubDirection with Id = {id} doesn't exist in the system.", id);
            return Result<SubDirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Deleting SubDirection with Id = {id} failed"
            });
        }
    }

    private async Task<List<OperationError>> SubDirectionValidation(long directionId, SubDirectionDto dto)
    {
        var errors = new List<OperationError>();

        if (await subDirectionRepository.Get(whereExpression: x => x.DirectionId == directionId && x.Title == dto.Title).AnyAsync())
        {
            logger.LogWarning("There is already a SubDirection with such a data.");

            errors.Add(new OperationError()
            {
                Code = "400",
                Description = "There is already a SubDirection with such a data."
            });
        }

        return errors;
    }
}
