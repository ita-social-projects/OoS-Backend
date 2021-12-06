using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    class ElasticsearchSyncRecordRepository : SensitiveEntityRepository<ElasticsearchSyncRecord>, IElasticsearchSyncRecordRepository
    {
        private readonly OutOfSchoolDbContext db;

        public ElasticsearchSyncRecordRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }
    }
}
