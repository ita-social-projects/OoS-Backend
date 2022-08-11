using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ChangesLogRepository : EntityRepository<long, ChangesLog>, IChangesLogRepository
{
    public ChangesLogRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    public ICollection<ChangesLog> AddChangesLogToDbContext<TEntity>(
        TEntity entity,
        string userId,
        IEnumerable<string> trackedProperties,
        Func<Type, object, string> valueProjector)
        where TEntity : class, IKeyedEntity, new()
    {
        _ = trackedProperties ?? throw new ArgumentNullException(nameof(trackedProperties));
        _ = valueProjector ?? throw new ArgumentNullException(nameof(valueProjector));

        var result = new List<ChangesLog>();
        var entry = dbContext.Entry(entity);

        var entityType = typeof(TEntity).Name;
        var (entityIdGuid, entityIdLong) = GetEntityId(entry);
        var changedValues = GetChangedValues(entry, trackedProperties, valueProjector);
        foreach (var (propertyName, oldValue, newValue) in changedValues)
        {
            result.Add(CreateChangesLogRecord(
                entityType,
                propertyName,
                entityIdGuid,
                entityIdLong,
                oldValue,
                newValue,
                userId));
        }

        if (result.Count > 0)
        {
            dbContext.AddRange(result);
        }

        return result;
    }

    // TODO: logging of the Institution changes is yet to be configured
    private IEnumerable<(string PropertyName, string OldValue, string NewValue)> GetChangedValues(
        EntityEntry entityEntry,
        IEnumerable<string> trackedProperties,
        Func<Type, object, string> valueProjector)
    {
        var properties = entityEntry.Properties
            .Where(p => p.IsModified
                        && trackedProperties.Contains(p.Metadata.Name))
            .Select(x => (x.Metadata.Name,
                valueProjector(x.Metadata.ClrType, x.OriginalValue),
                valueProjector(x.Metadata.ClrType, x.CurrentValue)));

        var references = entityEntry.References
            .Where(x => trackedProperties.Contains(x.Metadata.Name)
                        && x.TargetEntry?.State == EntityState.Modified)
            .Select(x => (x.Metadata.Name,
                valueProjector(x.TargetEntry.Metadata.ClrType, x.TargetEntry.OriginalValues.ToObject()),
                valueProjector(x.TargetEntry.Metadata.ClrType, x.TargetEntry.CurrentValues.ToObject())));

        var references2 = entityEntry.References
            .Where(x => trackedProperties.Contains(x.Metadata.Name)
                        && x.TargetEntry?.State == EntityState.Modified);

        return properties.Concat(references);
    }

    private (Guid? entityIdGuid, long? entityIdLong) GetEntityId(EntityEntry entityEntry)
    {
        Guid? entityIdGuid = null;
        long? entityIdLong = null;

        var idProperty = entityEntry.Property("Id");

        if (idProperty.Metadata.ClrType == typeof(Guid))
        {
            entityIdGuid = (Guid)idProperty.CurrentValue;
        }
        else if (idProperty.Metadata.ClrType == typeof(long))
        {
            entityIdLong = (long)idProperty.CurrentValue;
        }
        else
        {
            throw new ArgumentException($"Id field type '{idProperty.Metadata.ClrType.Name}' is not supported for logging");
        }

        return (entityIdGuid, entityIdLong);
    }

    private ChangesLog CreateChangesLogRecord(
        string entityType,
        string propertyName,
        Guid? entityIdGuid,
        long? entityIdLong,
        string oldValue,
        string newValue,
        string userId)
        => new ChangesLog
        {
            EntityType = entityType,
            PropertyName = propertyName,
            EntityIdGuid = entityIdGuid,
            EntityIdLong = entityIdLong,
            OldValue = oldValue.Limit(dbContext.GetPropertyMaxLength<ChangesLog>(nameof(ChangesLog.OldValue)) ?? 0),
            NewValue = newValue.Limit(dbContext.GetPropertyMaxLength<ChangesLog>(nameof(ChangesLog.NewValue)) ?? 0),
            UpdatedDate = DateTime.Now,
            UserId = userId,
        };
}