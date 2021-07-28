using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class BackupTrackerDto
    {
        public Guid Id { get; set; }

        public DateTime OperationDate { get; set; }

        public string TableName { get; set; }

        public long RecordId { get; set; }

        public BackupOperation Operation { get; set; }
    }
}
