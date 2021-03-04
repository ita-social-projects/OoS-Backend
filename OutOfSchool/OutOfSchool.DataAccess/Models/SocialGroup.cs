using System.Collections.Generic;


namespace OutOfSchool.Services.Models
{
    public class SocialGroup
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public SocialGroup()
        {
            Children = new List<Child>();
        }

    }
}
