using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Parent
    {
        public long ParentId { get; set; }
        
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Middle name is required")]
        public string MiddleName { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public Parent()
        {
            Children = new List<Child>();
        }
    }
}