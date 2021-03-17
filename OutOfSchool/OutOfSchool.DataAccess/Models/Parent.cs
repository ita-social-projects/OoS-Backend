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

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Middle name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; }

        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; }
    }
}