using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Provider : IKeyedEntity<Guid>
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

        [Required(ErrorMessage = "EDRPOU/IPN code is required")]
        [RegularExpression(
            @"^(\d{8}|\d{10})$",
            ErrorMessage = "EDRPOU/IPN code must contain 8 or 10 digits")]
        [MaxLength(12)]
        public long EdrpouIpn { get; set; }

        [MaxLength(50)]
        public string Director { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
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

        [Required]
        public OwnershipType Ownership { get; set; }

        [Required]
        public ProviderType Type { get; set; }

        public bool Status { get; set; } = default;

        [Required]
        public string UserId { get; set; }

        public virtual List<Workshop> Workshops { get; set; }

        public virtual User User { get; set; }

        public long? ActualAddressId { get; set; }

        public virtual Address ActualAddress { get; set; }

        [Required]
        public long LegalAddressId { get; set; }

        [Required]
        public virtual Address LegalAddress { get; set; }

        public long? InstitutionStatusId { get; set; }

        public virtual InstitutionStatus InstitutionStatus { get; set; }

        public virtual ICollection<ProviderAdmin> ProviderAdmins { get; set; }

        [Required]
        public InstitutionType InstitutionType { get; set; }
    }
}
