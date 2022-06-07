using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Changes
{
    public class ChangesLogFilter : OffsetFilter
    {
        [Required]
        public string EntityType { get; set; }

        public string FieldName { get; set; }

        public string EntityId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
