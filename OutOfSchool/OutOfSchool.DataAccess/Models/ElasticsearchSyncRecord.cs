using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class ElasticsearchSyncRecord : IKeyedEntity<Guid>
    {
        public Guid Id { get; set; }

        public ElasticsearchSyncEntity Entity { get; set; }

        public Guid RecordId { get; set; }

        public DateTimeOffset OperationDate { get; set; }

        public ElasticsearchSyncOperation Operation { get; set; }
    }
}
