using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IElasticsearchSyncRecordRepository : IEntityRepositoryBase<Guid, ElasticsearchSyncRecord>
    {
    }
}
