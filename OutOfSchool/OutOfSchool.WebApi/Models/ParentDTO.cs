using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class ParentDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\W\-\']*", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Middle name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\W\-\']*", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\W\-\']*", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; }

        public long UserId { get; set; }
    }
}
