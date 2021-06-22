using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Parent
    {
        public Parent()
        {
            Children = new List<Child>();
        }

        public long Id { get; set; }

        public virtual IReadOnlyCollection<Child> Children { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}