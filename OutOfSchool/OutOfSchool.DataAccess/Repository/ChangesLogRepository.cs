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
            var log = new List<ChangesLog>();
            var entry = dbContext.Entry(entity);

            var table = entry.GetTableName();
            if (!config.Value.SupportedFields.TryGetValue(table, out var supportedFields))
            {
                return log;
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
                            log.Add(CreateChangesLogRecord(prop, table, field, userId, entityIdGuid, entityIdLong));
                        }
                    }
                }
            }

            if (log.Count > 0)
            {
                dbContext.AddRange(log);
            }

            return log;
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
            PropertyEntry propertyEntry,
            string table,
            string field,
            string userId,
            Guid? recordIdGuid,
            long? recordIdLong)
        {
            var changesRecord = new ChangesLog
            {
                Table = table,
                Field = field,
                RecordIdGuid = recordIdGuid,
                RecordIdLong = recordIdLong,
                OldValue = propertyEntry.OriginalValue.ToString(),
                NewValue = propertyEntry.CurrentValue.ToString(),
                Changed = DateTime.Now,
                UserId = userId,
            };

            return changesRecord;
        }
    }
}
