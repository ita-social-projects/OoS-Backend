using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class ChangesLog : IKeyedEntity<long>
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string EntityType { get; set; }

        [Required]
        [MaxLength(128)]
        public string FieldName { get; set; }

        public Guid? EntityIdGuid { get; set; }

        public long? EntityIdLong { get; set; }

        [MaxLength(500)]
        public string OldValue { get; set; }

        [MaxLength(500)]
        public string NewValue { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }

        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
