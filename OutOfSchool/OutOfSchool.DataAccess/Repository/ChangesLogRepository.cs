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

        public IEnumerable<ChangesLog> AddChangesLogToDbContext<TEntity>(TEntity entity, string userId)
            where TEntity : class, new()
        {
            var log = new List<ChangesLog>();
            var entry = dbContext.Entry(entity);

            if (entry.State == EntityState.Modified)
            {
                var properties = entry.Properties.Where(p => p.IsModified).ToList();

                if (properties.Any())
                {
                    var (entityIdGuid, entityIdLong) = GetEntityId(entry);
                    var table = entry.GetTableName();

                    foreach (var prop in properties)
                    {
                        log.Add(CreateChangesLogRecord(prop, table, userId, entityIdGuid, entityIdLong));
                    }

                    dbContext.AddRange(log);
                }
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
            string userId,
            Guid? recordIdGuid,
            long? recordIdLong)
        {
            var changesRecord = new ChangesLog
            {
                Table = table,
                Field = propertyEntry.GetColumnName(),
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
