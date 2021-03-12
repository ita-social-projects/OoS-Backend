using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class SocialGroup
    {
        public SocialGroup()
        {
            Children = new List<Child>();
        }

        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual IReadOnlyCollection<Child> Children { get; set; }
    }
}
