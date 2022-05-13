using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class ChangesLog : IKeyedEntity<long>
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Table { get; set; }

        [Required]
        [MaxLength(128)]
        public string Field { get; set; }

        public Guid? RecordIdGuid { get; set; }

        public long? RecordIdLong { get; set; }

        [MaxLength(500)]
        public string OldValue { get; set; }

        [MaxLength(500)]
        public string NewValue { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Changed { get; set; }

        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
