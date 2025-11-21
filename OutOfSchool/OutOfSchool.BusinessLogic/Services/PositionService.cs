using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class PositionService(
    IPositionRepository positionRepository,
    ICurrentUserService currentUserService,
    ILogger<PositionService> logger
) : IPositionService
{
    public async Task<PositionDto> CreateAsync(PositionCreateUpdateDto createDto, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId), new DeputyDirectorRights(providerId));

        var position = createDto.ToModel();
        position.ProviderId = providerId;
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        position.ActiveFrom = now;
        position.ActiveTo = new DateOnly(2999, 12, 31);
        var createdPosition = await positionRepository.Create(position);
        logger.LogDebug("Created position with id: {PositionId}", position.Id);
        return createdPosition.ToDto();
    }

    public async Task<SearchResult<PositionDto>> GetByFilter(Guid providerId, PositionsFilter filter)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));

        logger.LogInformation("Getting all Positions started (by filter)");

        filter ??= new PositionsFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var predicate = PredicateBuilder.True<Position>();

        // Filter by FullName
        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            predicate = predicate.And(p =>
                                      p.FullName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase) ||
                                      p.ShortName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));

        }

        // Filter out deleted positions and take only positions for given provider
        predicate = predicate.And(p => !p.IsDeleted)
            .And(p => p.ProviderId == providerId);

        // Define sorting
        var sortPredicate = SortExpressionBuild(filter);

        var count = await positionRepository.Count(whereExpression: predicate).ConfigureAwait(false);

        // No nested entities in use – eager loading not required.
        var positions = await positionRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate,
                orderBy: sortPredicate)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation("Retrieved {PositionsCount} positions", positions.Count);

        var result = new SearchResult<PositionDto>
        {
            TotalAmount = count,
            Entities = positions.ToDto(),
        };

        return result;
    }

    public async Task<PositionDto> GetByIdAsync(Guid positionId, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));
        var position = await GetPositionAsync(positionId, providerId);

        return position?.ToDto();
    }

    public async Task<PositionDto> UpdateAsync(Guid positionId, PositionCreateUpdateDto updateDto, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));
        var existingPosition = await GetPositionAsync(positionId, providerId);
        if (existingPosition == null)
        {
            throw new KeyNotFoundException($"Position with id {positionId} not found");
        }

        var updatedPosition = await positionRepository.Update(updateDto.SetToModel(existingPosition));

        return updatedPosition.ToDto();
    }

    public async Task DeleteAsync(Guid positionId, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));

        var position = await GetPositionAsync(positionId, providerId);
        if (position == null)
        {
            return;
        }

        if (position.PositionType == PositionType.Director)
        {
            throw new InvalidOperationException("Cannot delete a director");
        }

        logger.LogInformation("Deleting position {PositionId} for provider {ProviderId}", positionId, providerId);
        await positionRepository.Delete(position);
    }

    private async Task<Position> GetPositionAsync(Guid positionId, Guid providerId)
    {
        // No nested entities in use – eager loading not required.
        var position = await positionRepository.GetByFilter(
                x => x.Id == positionId && x.ProviderId == providerId && !x.IsDeleted)
            .ConfigureAwait(false);

        return position.SingleOrDefault();
    }

    private static Dictionary<Expression<Func<Position, object>>, SortDirection> SortExpressionBuild(
        PositionsFilter filter)
    {
        var sortExpression = new Dictionary<Expression<Func<Position, object>>, SortDirection>();

        switch (filter.FilterByProperty?.ToLower())
        {
            case string property when property.Equals(nameof(Position.FullName).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(a => a.FullName, filter.Order? SortDirection.Ascending : SortDirection.Descending); break;

            case string property when property.Equals(nameof(Position.PositionRate).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(a => a.PositionRate, filter.Order ? SortDirection.Ascending : SortDirection.Descending); break;

            case string property when property.Equals(nameof(Position.SeatsAmount).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(a => a.SeatsAmount, filter.Order ? SortDirection.Ascending : SortDirection.Descending); break;

            case string property when property.Equals(nameof(Position.Tariff).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(a => a.Tariff, filter.Order ? SortDirection.Ascending : SortDirection.Descending); break;

            case string property when property.Equals(nameof(Position.CreatedAt).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(a => a.CreatedAt, filter.Order ? SortDirection.Ascending : SortDirection.Descending); break;
            
            default:
                sortExpression.Add(a => a.CreatedAt, SortDirection.Descending); break;
        }

        return sortExpression;
    }
}