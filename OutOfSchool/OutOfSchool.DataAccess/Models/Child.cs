using System;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Child
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public string PlaceOfStudy { get; set; }

        // TODO: validate case when child has registered without parent help
        public long ParentId { get; set; }

        public virtual Parent Parent { get; set; }

        public long? SocialGroupId { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }
    }
}