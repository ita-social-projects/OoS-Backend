using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ElasticsearchSyncRecordRepository : SensitiveEntityRepository<ElasticsearchSyncRecord>, IElasticsearchSyncRecordRepository
    {

        public ElasticsearchSyncRecordRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<IEnumerable<ElasticsearchSyncRecord>> GetByEntity(
            ElasticsearchSyncEntity elasticsearchSyncEntity,
            int numberOfRecords)
        {
            return await dbSet
                .Where(es => es.Entity == elasticsearchSyncEntity)
                .OrderBy(order => order.OperationDate)
                .Take(numberOfRecords).ToListAsync();
        }

        public async Task DeleteRange(IEnumerable<Guid> ids)
        {
            dbSet.RemoveRange(dbSet.Where(es => ids.Contains(es.Id)));

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
