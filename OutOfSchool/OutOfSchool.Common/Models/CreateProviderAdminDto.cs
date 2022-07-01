using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Common.Models
{
    public class CreateProviderAdminDto
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
            Constants.PhoneNumberRegexModel,
            ErrorMessage = Constants.PhoneErrorMessage)]
        [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
        [MaxLength(Constants.UnifiedPhoneLength)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatingTime { get; set; }

        public string ReturnUrl { get; set; }

        public Guid ProviderId { get; set; }

        public string UserId { get; set; }

        // to specify if its assistant or deputy
        public bool IsDeputy { get; set; }

        // to specify workshops, which can be managed by provider admin
        public List<Guid> ManagedWorkshopIds { get; set; }
    }
}