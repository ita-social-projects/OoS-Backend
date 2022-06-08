using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    public class ChangesLogService : IChangesLogService
    {
        private readonly IOptions<ChangesLogConfig> config;
        private readonly IChangesLogRepository changesLogRepository;
        private readonly IProviderRepository providerRepository;
        private readonly ILogger<ChangesLogService> logger;
        private readonly IMapper mapper;

        public ChangesLogService(
            IOptions<ChangesLogConfig> config,
            IChangesLogRepository changesLogRepository,
            IProviderRepository providerRepository,
            ILogger<ChangesLogService> logger,
            IMapper mapper)
        {
            this.config = config;
            this.changesLogRepository = changesLogRepository;
            this.providerRepository = providerRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        public int AddEntityChangesToDbContext<TEntity>(TEntity entity, string userId)
            where TEntity : class, IKeyedEntity, new()
        {
            if (!IsLoggingAllowed<TEntity>(out var trackedFields))
            {
                logger.LogDebug($"Logging is not allowed for the '{typeof(TEntity).Name}' entity type.");

                return 0;
            }

            logger.LogDebug($"Logging of the '{typeof(TEntity).Name}' entity changes started.");

            var result = changesLogRepository.AddChangesLogToDbContext(entity, userId, trackedFields);

            logger.LogDebug($"Added {result.Count} records to the Changes Log.");

            return result.Count;
        }

        public int AddEntityAddressChangesLogToDbContext<TEntity>(TEntity entity, string addressPropertyName, string userId)
            where TEntity : class, IKeyedEntity, new()
            => AddPropertyChangesLogToDbContext<TEntity, Address>(entity, addressPropertyName, ProjectAddress, userId);

        public async Task<SearchResult<ProviderChangesLogDto>> GetProviderChangesLogAsync(ProviderChangesLogRequest request)
        {
            var (changesLog, count) = await GetChangesLogAsync(mapper.Map<ChangesLogFilter>(request))
                .ConfigureAwait(false);
            var providers = providerRepository.Get<int>();

            var query = from l in changesLog
                        join p in providers
                            on l.EntityIdGuid equals p.Id
                        select new ProviderChangesLogDto
                        {
                            FieldName = l.FieldName,
                            OldValue = l.OldValue,
                            NewValue = l.NewValue,
                            UpdatedDate = l.UpdatedDate,
                            User = mapper.Map<ShortUserDto>(l.User),
                            ProviderId = l.EntityIdGuid.Value,
                            ProviderTitle = p.FullTitle,
                            ProviderCity = p.LegalAddress.City,
                        };

            var entities = await query.ToListAsync().ConfigureAwait(false);

            return new SearchResult<ProviderChangesLogDto>
            {
                Entities = entities,
                TotalAmount = count,
            };
        }

        private async Task<(IQueryable<ChangesLog>, int)> GetChangesLogAsync(ChangesLogFilter filter)
        {
            ValidateFilter(filter);

            var where = GetQueryFilter(filter);
            var (orderBy, ascending) = GetOrderParams();

            var count = await changesLogRepository.Count(where).ConfigureAwait(false);
            var query = changesLogRepository.Get(filter.From, filter.Size, "User", where, orderBy, ascending, true);

            return (query, count);
        }

        private int AddPropertyChangesLogToDbContext<TEntity, TProperty>(
            TEntity entity,
            string propertyName,
            Func<TProperty, string> valueProjector,
            string userId)
            where TEntity : class, IKeyedEntity, new()
        {
            if (!IsLoggingAllowed<TEntity>(propertyName))
            {
                logger.LogDebug($"Logging is not allowed for the '{typeof(TEntity).Name}.{propertyName}' field.");

                return 0;
            }

            logger.LogDebug($"Logging of the '{typeof(TEntity).Name}.{propertyName}' changes started.");

            var result = changesLogRepository
                .AddPropertyChangesLogToDbContext<TEntity, TProperty>(entity, propertyName, valueProjector, userId);

            var count = result == null ? 0 : 1;

            logger.LogDebug($"Added {count} records to the Changes Log.");

            return count;
        }

        private bool IsLoggingAllowed<TEntity>(out string[] supportedFields)
            => config.Value.TrackedFields.TryGetValue(typeof(TEntity).Name, out supportedFields);

        private bool IsLoggingAllowed<TEntity>(string addressPropertyName)
            => IsLoggingAllowed<TEntity>(out var supportedFields)
                && supportedFields.Contains(addressPropertyName);

        private Expression<Func<ChangesLog, bool>> GetQueryFilter(ChangesLogFilter filter)
        {
            Expression<Func<ChangesLog, bool>> expr = x => x.EntityType == filter.EntityType;

            if (filter.FieldName != null)
            {
                expr = expr.And(x => x.FieldName == filter.FieldName);
            }

            if (filter.EntityId != null)
            {
                if (Guid.TryParse(filter.EntityId, out var recordIdGuid))
                {
                    expr = expr.And(x => x.EntityIdGuid == recordIdGuid);
                }
                else if (long.TryParse(filter.EntityId, out var recordIdLong))
                {
                    expr = expr.And(x => x.EntityIdLong == recordIdLong);
                }
            }

            if (filter.DateFrom.HasValue)
            {
                expr = expr.And(x => x.UpdatedDate >= filter.DateFrom.Value.Date);
            }

            if (filter.DateTo.HasValue)
            {
                expr = expr.And(x => x.UpdatedDate < filter.DateTo.Value.Date.AddDays(1));
            }

            return expr;
        }

        private (Expression<Func<ChangesLog, dynamic>> orderBy, bool ascending) GetOrderParams()
        {
            // Returns default ordering so far...
            Expression<Func<ChangesLog, dynamic>> orderBy = x => x.UpdatedDate;

            return (orderBy, false);
        }

        private void ValidateFilter(ChangesLogFilterBase filter)
        {
            ModelValidationHelper.ValidateOffsetFilter(filter);
        }

        private string ProjectAddress(Address address) =>
            address == null
            ? null
            : $"{address.District}, {address.City}, {address.Region}, {address.Street}, {address.BuildingNumber}";
    }
}
