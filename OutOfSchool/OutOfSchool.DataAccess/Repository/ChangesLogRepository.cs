using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ChangesLogRepository : EntityRepository<ChangesLog>, IChangesLogRepository
    {
        public ChangesLogRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
        }

        public ICollection<ChangesLog> AddChangesLogToDbContext<TEntity>(TEntity entity, string userId, IEnumerable<string> trackedFields)
            where TEntity : class, IKeyedEntity, new()
        {
            var result = new List<ChangesLog>();
            var entry = dbContext.Entry(entity);

            if (entry.State != EntityState.Modified)
            {
                return result;
            }

            var entityType = typeof(TEntity).Name;
            var (entityIdGuid, entityIdLong) = GetEntityId(entry);
            foreach (var prop in entry.Properties.Where(p => p.IsModified))
            {
                var fieldName = prop.GetColumnName();
                if (trackedFields.Contains(fieldName))
                {
                    result.Add(CreateChangesLogRecord(
                        entityType,
                        fieldName,
                        entityIdGuid,
                        entityIdLong,
                        prop.OriginalValue.ToString(),
                        prop.CurrentValue.ToString(),
                        userId));
                }
            }

            if (result.Count > 0)
            {
                dbContext.AddRange(result);
            }

            return result;
        }

        public ChangesLog AddPropertyChangesLogToDbContext<TEntity, TProperty>(
            TEntity entity,
            string propertyName,
            Func<TProperty, string> valueProjector,
            string userId)
            where TEntity : class, IKeyedEntity, new()
        {
            ChangesLog result = null;
            var entry = dbContext.Entry(entity);
            var propertyEntityEntry = entry.Reference(propertyName).TargetEntry;

            if (propertyEntityEntry.State != EntityState.Modified)
            {
                return result;
            }

            var entityType = typeof(TEntity).Name;
            var (entityIdGuid, entityIdLong) = GetEntityId(entry);

            result = CreateChangesLogRecord(
                entityType,
                propertyName,
                entityIdGuid,
                entityIdLong,
                valueProjector((TProperty)propertyEntityEntry.OriginalValues.ToObject()),
                valueProjector((TProperty)propertyEntityEntry.CurrentValues.ToObject()),
                userId);

            if (result != null)
            {
                dbContext.Add(result);
            }

            return result;
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
            string fieldName,
            Guid? entityIdGuid,
            long? entityIdLong,
            string oldValue,
            string newValue,
            string userId)
            => new ChangesLog
            {
                EntityType = entityType,
                FieldName = fieldName,
                EntityIdGuid = entityIdGuid,
                EntityIdLong = entityIdLong,
                OldValue = oldValue,
                NewValue = newValue,
                UpdatedDate = DateTime.Now,
                UserId = userId,
            };
    }
}
