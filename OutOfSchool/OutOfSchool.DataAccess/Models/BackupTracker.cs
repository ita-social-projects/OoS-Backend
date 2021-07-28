using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class BackupTracker
    {
        public Guid Id { get; set; }

        public DateTime OperationDate { get; set; }

        public string TableName { get; set; }

        public long RecordId { get; set; }

        public BackupOperation Operation { get; set; }
    }
}
