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

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;
        
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;
        
        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public User User { get; set; }
    }
}