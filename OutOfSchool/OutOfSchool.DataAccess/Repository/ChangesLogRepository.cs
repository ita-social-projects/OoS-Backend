using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class ChangesLogRepository : EntityRepository<long, ChangesLog>, IChangesLogRepository
{
    public const string CreatingOperation = "Creating";

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

        string propertyNameTmp = string.Empty;
        string oldValueTmp = string.Empty;
        string newValueTmp = string.Empty;

        foreach (var (propertyName, oldValue, newValue) in changedValues)
        {
            propertyNameTmp = propertyName;
            oldValueTmp = oldValue;
            newValueTmp = newValue;

            if (propertyName == "InstitutionId")
            {
                propertyNameTmp = "Institution";
                oldValueTmp = dbContext.Institutions.Where(x => x.Id == Guid.Parse(oldValue)).Single().Title;
                newValueTmp = dbContext.Institutions.Where(x => x.Id == Guid.Parse(newValue)).Single().Title;
            }

            if (propertyName == "InstitutionStatusId")
            {
                propertyNameTmp = "InstitutionStatus";
                oldValueTmp = dbContext.InstitutionStatuses.Where(x => x.Id == long.Parse(oldValue)).Single().Name;
                newValueTmp = dbContext.InstitutionStatuses.Where(x => x.Id == long.Parse(newValue)).Single().Name;
            }

            result.Add(CreateChangesLogRecord(
                entityType,
                propertyNameTmp,
                entityIdGuid,
                entityIdLong,
                oldValueTmp,
                newValueTmp,
                userId));
        }

        if (result.Count > 0)
        {
            dbContext.AddRange(result);
        }

        return result;
    }

    public Task<ChangesLog> AddCreatingOfEntityToChangesLog<TEntity>(
        TEntity entity,
        string userId)
        where TEntity : class, IKeyedEntity, new()
    {
        var entry = dbContext.Entry(entity);
        var (entityIdGuid, entityIdLong) = GetEntityId(entry);

        var changesLog = CreateChangesLogRecord(
            typeof(TEntity).Name,
            CreatingOperation,
            entityIdGuid,
            entityIdLong,
            string.Empty,
            string.Empty,
            userId);

        return Create(changesLog);
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

        var ownedEntities = entityEntry.Navigations
            .Where(n => trackedProperties.Contains(n.Metadata.Name)
                        && n.Metadata.TargetEntityType.IsOwned() 
                        && n.EntityEntry.State == EntityState.Modified)
            .Select(n => (
                PropertyName: n.Metadata.Name,
                OldValue: valueProjector(n.EntityEntry.Metadata.ClrType, n.EntityEntry.OriginalValues.ToObject()),
                NewValue: valueProjector(n.EntityEntry.Metadata.ClrType, n.EntityEntry.CurrentValues.ToObject())
            ));

        // For owned collections (Contacts only)
        var ownedCollectionChanges = entityEntry.Collections
            .Where(c => c.Metadata.Name == "Contacts"
                        && c.Metadata.TargetEntityType.IsOwned())
            .SelectMany(c =>
            {
                // Cast the current collection value to IEnumerable<object>
                if (c.CurrentValue is not IEnumerable<object> collection)
                    return [];

                return collection.SelectMany(item =>
                {
                    var changes = new List<(string PropertyName, string OldValue, string NewValue)>();
                    var ownedEntry = entityEntry.Context.Entry(item);

                    if (c.Metadata.Name == "Contacts")
                    {
                        var defaultContact = collection.FirstOrDefault(contact =>
                            ((dynamic) contact).IsDefault == true);

                        if (defaultContact != null)
                        {
                            var contactEntry = entityEntry.Context.Entry(defaultContact);
                            var addressEntry = contactEntry.Reference("Address").TargetEntry;
                            if (addressEntry?.Properties
                                    .Any(p => p.IsModified) == true)
                            {
                                var originalAddress = addressEntry.OriginalValues.ToObject();
                                var currentAddress = addressEntry.CurrentValues.ToObject();
                                
                                changes.Add((
                                    PropertyName: "Contacts.Address",
                                    OldValue: valueProjector(typeof(ContactsAddress), originalAddress),
                                    NewValue: valueProjector(typeof(ContactsAddress), currentAddress)
                                ));
                            }
                        }
                    }

                    return changes;
                });
            });

        return properties.Concat(references)
            .Concat(ownedEntities)
            .Concat(ownedCollectionChanges);
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
            UpdatedDate = DateTime.UtcNow,
            UserId = userId,
        };
}