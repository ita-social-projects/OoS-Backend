using System;

namespace OutOfSchool.Services.Models
{
    public class ChildSocialGroup
    {
        public Guid ChildId { get; set; }

        public virtual Child Child { get; set; }

        public long SocialGroupId { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }
    }
}