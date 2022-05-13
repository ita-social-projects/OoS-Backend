using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChangesLogFilter : OffsetFilter
    {
        [Required]
        public string Table { get; set; }

        public string Field { get; set; }

        public string RecordId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
