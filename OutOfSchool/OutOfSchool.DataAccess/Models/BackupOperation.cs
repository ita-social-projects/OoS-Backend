using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class BackupOperation
    {
        public Guid Id { get; set; }

        public DateTime OperationDate { get; set; }

        public string TableName { get; set; }

        public long RecordId { get; set; }

        public Operations Operation { get; set; }
    }
}
