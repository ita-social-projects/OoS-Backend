using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class ElasticsearchSyncRecord
    {
        public Guid Id { get; set; }

        public ElasticsearchSyncEntity Entity { get; set; }

        public long RecordId { get; set; }

        public DateTime OperationDate { get; set; }

        public ElasticsearchSyncOperation Operation { get; set; }
    }
}
