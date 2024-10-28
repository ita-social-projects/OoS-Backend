using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class OperationWithObjectService : IOperationWithObjectService
{
    private readonly ILogger<OperationWithObjectService> logger;
    private readonly IMapper mapper;
    private readonly ISensitiveEntityRepository<OperationWithObject> operationWithObjectRepository;

    public OperationWithObjectService(
        ILogger<OperationWithObjectService> logger,
        IMapper mapper,
        ISensitiveEntityRepository<OperationWithObject> operationWithObjectRepository)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.operationWithObjectRepository = operationWithObjectRepository;
    }

    /// <inheritdoc/>
    public async Task Create(
        OperationWithObjectOperationType operationType,
        Guid? entityId,
        OperationWithObjectEntityType? entityType,
        DateTimeOffset? eventDateTime = null,
        string rowSeparator = null,
        string comment = null)
    {
        logger.LogInformation("OperationWithObject (operationType: {operationType}, entityId: {entityId}) creating was started", operationType, entityId);

        var operationWithObject = new OperationWithObject
        {
            Id = Guid.NewGuid(),
            OperationType = operationType,
            EntityType = entityType,
            EntityId = entityId,
            EventDateTime = eventDateTime ?? DateTimeOffset.UtcNow,
            RowSeparator = rowSeparator ?? string.Empty,
            Comment = comment ?? string.Empty,
        };

        var operationWithObjectDto = await operationWithObjectRepository.Create(operationWithObject);

        logger.LogInformation("OperationWithObject with Id = {Id} was created successfully", operationWithObjectDto?.Id);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation("Deleting OperationWithObject with Id = {Id} started", id);

        var entity = await operationWithObjectRepository.GetById(id);

        if (entity is not null)
        {
            try {
                await operationWithObjectRepository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation("OperationWithObject with Id = {Id} successfully deleted", id);
            }
            catch (DbUpdateConcurrencyException) {
                logger.LogError("Deleting failed. OperationWithObject with Id = {Id} doesn't exist in the system", id);
                throw;
            }
        }
    }

    public async Task<IEnumerable<OperationWithObjectDto>> GetAll(OperationWithObjectFilter filter)
    {
        logger.LogInformation("Getting all OperationWithObject with filter");

        var predicate = PredicateBuild(filter);
        var operationsWithObjects = (await operationWithObjectRepository.GetByFilter(predicate)).ToList();

        logger.LogInformation(
            "{Count} records were successfully received from the OperationWithObject table.",
            operationsWithObjects.Count);

        return operationsWithObjects.Select(operation => mapper.Map<OperationWithObjectDto>(operation)).ToList();
    }

    public async Task<bool> Exists(OperationWithObjectFilter filter)
    {
        logger.LogInformation("Checking for existing entities with filter");

        var predicate = PredicateBuild(filter);
        return (await operationWithObjectRepository.Count(predicate)) != 0;
    }

    private static Expression<Func<OperationWithObject, bool>> PredicateBuild(OperationWithObjectFilter filter)
    {
        filter = filter ?? new OperationWithObjectFilter();

        var predicate = PredicateBuilder.True<OperationWithObject>();

        predicate = predicate.And(x => x.OperationType == filter.OperationType);

        if (filter.EntityId is not null)
        {
            predicate = predicate.And(x => x.EntityId == filter.EntityId);
        }

        if (filter.EntityType is not null)
        {
            predicate = predicate.And(x => x.EntityType == filter.EntityType);
        }

        if (filter.EventDateTime is not null)
        {
            predicate = predicate.And(x => x.EventDateTime == filter.EventDateTime);
        }

        if (!string.IsNullOrEmpty(filter.RowSeparator))
        {
            predicate = predicate.And(x => x.RowSeparator == filter.RowSeparator);
        }

        return predicate;
    }
}
