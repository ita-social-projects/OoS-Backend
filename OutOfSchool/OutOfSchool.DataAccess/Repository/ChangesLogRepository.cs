using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Configuration;

namespace OutOfSchool.Services.Repository
{
    public class ChangesLogRepository : EntityRepository<ChangesLog>, IChangesLogRepository
    {
        private readonly IOptions<ChangesLogConfig> config;

        public ChangesLogRepository(OutOfSchoolDbContext dbContext, IOptions<ChangesLogConfig> config)
            : base(dbContext)
        {
            this.config = config;
        }

        public ICollection<ChangesLog> AddChangesLogToDbContext<TEntity>(TEntity entity, string userId)
            where TEntity : class, new()
        {
            var result = new List<ChangesLog>();
            var entry = dbContext.Entry(entity);

            var table = entry.GetTableName();
            if (!config.Value.SupportedFields.TryGetValue(table, out var supportedFields))
            {
                return result;
            }

            if (entry.State == EntityState.Modified)
            {
                var properties = entry.Properties.Where(p => p.IsModified).ToList();

                if (properties.Any())
                {
                    var (entityIdGuid, entityIdLong) = GetEntityId(entry);

                    foreach (var prop in properties)
                    {
                        var field = prop.GetColumnName();
                        if (supportedFields.Contains(field))
                        {
                            result.Add(CreateChangesLogRecord(
                                table,
                                field,
                                entityIdGuid,
                                entityIdLong,
                                prop.OriginalValue.ToString(),
                                prop.CurrentValue.ToString(),
                                userId));
                        }
                    }
                }
            }

            if (result.Count > 0)
            {
                dbContext.AddRange(result);
            }

            return result;
        }

        public ChangesLog AddEntityAddressChangesLogToDbContext<TEntity>(
            TEntity entity,
            string addressField,
            Func<Address, string> addressProjector,
            string userId)
            where TEntity : class, new()
        {
            ChangesLog result = null;
            var entry = dbContext.Entry(entity);

            var table = entry.GetTableName();
            if (!config.Value.SupportedFields.TryGetValue(table, out var supportedFields) && !supportedFields.Contains(addressField))
            {
                return result;
            }

            var addressEntity = entry.Reference(addressField).TargetEntry;

            if (addressEntity.State == EntityState.Modified)
            {
                var (entityIdGuid, entityIdLong) = GetEntityId(entry);
                result = CreateChangesLogRecord(
                    table,
                    addressField,
                    entityIdGuid,
                    entityIdLong,
                    addressProjector((Address)addressEntity.OriginalValues.ToObject()),
                    addressProjector((Address)addressEntity.CurrentValues.ToObject()),
                    userId);
            }

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
            string table,
            string field,
            Guid? recordIdGuid,
            long? recordIdLong,
            string oldValue,
            string newValue,
            string userId)
        {
            var changesRecord = new ChangesLog
            {
                Table = table,
                Field = field,
                RecordIdGuid = recordIdGuid,
                RecordIdLong = recordIdLong,
                OldValue = oldValue,
                NewValue = newValue,
                Changed = DateTime.Now,
                UserId = userId,
            };

            return changesRecord;
        }
    }
}
