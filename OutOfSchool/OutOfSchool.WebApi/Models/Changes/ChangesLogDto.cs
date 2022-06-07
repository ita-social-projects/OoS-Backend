using System;

namespace OutOfSchool.WebApi.Models.Changes
{
    public class ChangesLogDto
    {
        public string EntityType { get; set; }

        public string FieldName { get; set; }

        public string EntityId { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public DateTime UpdatedDate { get; set; }

        public ShortUserDto User { get; set; }
    }
}
