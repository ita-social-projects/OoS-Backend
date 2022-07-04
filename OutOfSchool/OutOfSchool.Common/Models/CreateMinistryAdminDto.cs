using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Common.Models
{
    public class CreateMinistryAdminDto
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

        public Guid InstitutionId { get; set; }

        public long CodificatorId { get; set; }

        public string UserId { get; set; }
    }
}