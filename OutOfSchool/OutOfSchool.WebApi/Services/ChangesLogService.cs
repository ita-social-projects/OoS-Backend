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

        public async Task<IEnumerable<ChangesLogDto>> GetChangesLog(ChangesLogFilter filter)
        {
            // TODO: add filter validation
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            Expression<Func<ChangesLog, bool>> where = x =>
                x.Table == filter.Table
                && x.Field == filter.Field;

            if (Guid.TryParse(filter.RecordId, out var recordIdGuid))
            {
                where = where.And(x => x.RecordIdGuid == recordIdGuid);
            }
            else if (long.TryParse(filter.RecordId, out var recordIdLong))
            {
                where = where.And(x => x.RecordIdLong == recordIdLong);
            }

            Expression<Func<ChangesLog, DateTime>> orderBy = x => x.Changed;

            var log = await changesLogRepository
                .Get(filter.From, filter.Size, "User", where, orderBy, false)
                .ToListAsync()
                .ConfigureAwait(false);

            var result = mapper.Map<IEnumerable<ChangesLogDto>>(log);

            return result;
        }
    }
}
