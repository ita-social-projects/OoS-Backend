using System;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDTO
    {
        public long Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; } = default;

        public long ParentId { get; set; } = default;

        public long SocialGroupId { get; set; } = default;
    }
}
