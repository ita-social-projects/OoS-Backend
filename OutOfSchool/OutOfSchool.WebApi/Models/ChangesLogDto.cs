using System;

namespace OutOfSchool.WebApi.Models
{
    public class ChangesLogDto
    {
        public string Table { get; set; }

        public string Field { get; set; }

        public string RecordId { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public DateTime Changed { get; set; }

        public ShortUserDto User { get; set; }
    }
}
