using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IElasticsearchSyncRecordRepository : IEntityRepositoryBase<Guid, ElasticsearchSyncRecord>
{
    Task<IEnumerable<ElasticsearchSyncRecord>> GetByEntity(
        ElasticsearchSyncEntity elasticsearchSyncEntity,
        int numberOfRecords);

    Task DeleteRange(IEnumerable<Guid> ids);
}