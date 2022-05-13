using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    public class ChangesLogService : IChangesLogService
    {
        private readonly IChangesLogRepository changesLogRepository;
        private readonly ILogger<ChangesLogService> logger;
        private readonly IMapper mapper;

        public ChangesLogService(IChangesLogRepository changesLogRepository, ILogger<ChangesLogService> logger, IMapper mapper)
        {
            this.changesLogRepository = changesLogRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        public int AddEntityChangesToDbContext<TEntity>(TEntity entity, string userId)
            where TEntity : class, new()
        {
            logger.LogDebug($"Logging of '{typeof(TEntity).Name}' entity changes started.");

            var result = changesLogRepository.AddChangesLogToDbContext(entity, userId);

            logger.LogDebug($"Added {result.Count} records to the Changes Log.");

            return result.Count;
        }

        public async Task<SearchResult<ChangesLogDto>> GetChangesLog(ChangesLogFilter filter)
        {
            ValidateFilter(filter);

            var where = GetQueryFilter(filter);
            var (orderBy, ascending) = GetOrderParams(filter);

            var count = await changesLogRepository.Count(where).ConfigureAwait(false);
            var log = await changesLogRepository.Get(filter.From, filter.Size, "User", where, orderBy, ascending)
                .ToListAsync()
                .ConfigureAwait(false);

            var entities = mapper.Map<IReadOnlyCollection<ChangesLogDto>>(log);

            return new SearchResult<ChangesLogDto>
            {
                Entities = entities,
                TotalAmount = count,
            };
        }

        private Expression<Func<ChangesLog, bool>> GetQueryFilter(ChangesLogFilter filter)
        {
            Expression<Func<ChangesLog, bool>> expr = x => x.Table == filter.Table;

            if (filter.Field != null)
            {
                expr = expr.And(x => x.Field == filter.Field);
            }

            if (filter.RecordId != null)
            {
                if (Guid.TryParse(filter.RecordId, out var recordIdGuid))
                {
                    expr = expr.And(x => x.RecordIdGuid == recordIdGuid);
                }
                else if (long.TryParse(filter.RecordId, out var recordIdLong))
                {
                    expr = expr.And(x => x.RecordIdLong == recordIdLong);
                }
            }

            if (filter.DateFrom.HasValue)
            {
                expr = expr.And(x => x.Changed >= filter.DateFrom);
            }

            if (filter.DateTo.HasValue)
            {
                expr = expr.And(x => x.Changed <= filter.DateTo);
            }

            return expr;
        }

        private (Expression<Func<ChangesLog, DateTime>> orderBy, bool ascending) GetOrderParams(ChangesLogFilter filter)
        {
            // Returns default ordering so far...
            Expression<Func<ChangesLog, DateTime>> orderBy = x => x.Changed;

            return (orderBy, false);
        }

        private void ValidateFilter(ChangesLogFilter filter)
        {
            ModelValidationHelper.ValidateOffsetFilter(filter);
        }
    }
}
