using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class SocialGroup
    {
        public long SocialGroupId { get; set; }
        public string Name { get; set; }
        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public SocialGroup()
        {
            Children = new List<Child>();
        }

    }
}
