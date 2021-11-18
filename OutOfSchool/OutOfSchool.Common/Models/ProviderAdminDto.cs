using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Common.Models
{
    public class ProviderAdminDto
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
        @"([0-9]{2})([-]?)([0-9]{3})([-]?)([0-9]{2})([-]?)([0-9]{2})",
        ErrorMessage = "Phone number format is incorrect. Example: +380XX-XXX-XX-XX")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatingTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset? LastLogin { get; set; }

        public string ReturnUrl { get; set; }

        public string Role { get; set; }

        public Guid ProviderId { get; set; }

        public string UserId { get; set; }
    }
}
