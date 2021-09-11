using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models
{
    public class Teacher : IEquatable<Teacher>
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Middle name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(256)]
        public string Image { get; set; } = string.Empty;

        [Required]
        public long WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public bool Equals(Teacher other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && FirstName == other.FirstName && LastName == other.LastName && MiddleName == other.MiddleName && DateOfBirth.Equals(other.DateOfBirth) && Description == other.Description && Image == other.Image;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Teacher) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, FirstName, LastName, MiddleName, DateOfBirth, Description, Image);
        }
    }
}