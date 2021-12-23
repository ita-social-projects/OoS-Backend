using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IElasticsearchSyncRecordRepository : IEntityRepositoryBase<Guid, ElasticsearchSyncRecord>
    {
        Task<IEnumerable<ElasticsearchSyncRecord>> GetByEntity(
            ElasticsearchSyncEntity elasticsearchSyncEntity,
            int numberOfRecords);

        Task DeleteRange(IEnumerable<Guid> ids);
    }
}
