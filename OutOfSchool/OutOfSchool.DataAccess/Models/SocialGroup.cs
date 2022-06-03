using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class SocialGroup : IKeyedEntity<long>
    {
        public long Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual IReadOnlyCollection<Child> Children { get; set; } = new List<Child>();

        public virtual List<ChildSocialGroup> ChildSocialGroups { get; set; }
    }
}