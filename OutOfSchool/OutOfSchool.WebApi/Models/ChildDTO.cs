using System;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDTO
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long ParentId { get; set; }
        public long SocialGroupId { get; set; }
    }
}
