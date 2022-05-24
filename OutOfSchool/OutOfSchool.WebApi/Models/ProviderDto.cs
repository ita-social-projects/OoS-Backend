using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ProviderDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string FullTitle { get; set; }

        [Required(ErrorMessage = "Short Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string ShortTitle { get; set; }

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Website { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Facebook { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Instagram { get; set; } = string.Empty;

        [MaxLength(500)]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        // TODO: validate regex with unit tests
        [Required(ErrorMessage = "EDRPOU/INP code is required")]
        [RegularExpression(
            @"^(\d{8}|\d{10})$",
            ErrorMessage = "EDRPOU/IPN code must contain 8 or 10 digits")]
        public string EdrpouIpn { get; set; }

        [MaxLength(50)]
        public string Director { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DirectorDateOfBirth { get; set; } = default;

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(
            Constants.PhoneNumberRegexModel,
            ErrorMessage = Constants.PhoneErrorMessage)]
        [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
        [MaxLength(Constants.UnifiedPhoneLength)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Founder { get; set; } = string.Empty;

        // TODO: validation of the enum fields ?
        [Required]
        public InstitutionType Institution { get; set; }

        // TODO: validation of the enum fields ?
        [Required]
        public OrganizationType Type { get; set; }

        public bool Status { get; set; } = default;

        public float Rating { get; set; }

        public int NumberOfRatings { get; set; }

        // TODO: Does not used by front-end, can be removed.
        //       Unit test should be updated
        [Required]
        public string UserId { get; set; }

        [Required]
        public AddressDto LegalAddress { get; set; }

        public AddressDto ActualAddress { get; set; }

        public long? InstitutionStatusId { get; set; } = default;
    }
}