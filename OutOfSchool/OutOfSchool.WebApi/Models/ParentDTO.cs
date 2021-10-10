using System;

namespace OutOfSchool.WebApi.Models
{
    public class ParentDTO
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
