using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Models
{
    public class ProviderDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string Title { get; set; }

        [Required(ErrorMessage = "ShortTitle is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string ShortTitle { get; set; }

        [DataType(DataType.Url)]
        [MaxLength(100)]
        public string Website { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(100)]
        public string Facebook { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(100)]
        public string Instagram { get; set; } = string.Empty;

        [MaxLength(500)]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "MFO code is required")]
        [RegularExpression(@"(^[0-9]{1,6}$)", ErrorMessage = "Invalid MFO format. Example: XXX XXX")]
        [MinLength(6, ErrorMessage = "MFO code must contain 6 digits")]
        public string MFO { get; set; } = string.Empty;

        [Required(ErrorMessage = "EDRPOU code is required")]
        [RegularExpression(
            @"^(\d{8}|\d{10})$",
            ErrorMessage = "EDRPOU code must contain 8 or 10 digits")]
        public string EDRPOU { get; set; } = string.Empty;

        [RegularExpression(
            @"^(\d{10})$",
            ErrorMessage = "KOATUU code must contain 10 digits")]
        public string KOATUU { get; set; } = string.Empty;

        [Required(ErrorMessage = "INPP code is required")]
        [RegularExpression(
            @"^(\d{9}|\d{10}|\d{12})$",
            ErrorMessage = "INPP code must contain 12, 10 or 9 digits")]
        public string INPP { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Director { get; set; } = string.Empty;

        [MaxLength(255)]
        public string DirectorPosition { get; set; } = string.Empty;

        [MaxLength(128)]
        public string AuthorityHolder { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DirectorBirthDay { get; set; } = default;

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(
            @"([\d]{9})",
            ErrorMessage = "Phone number format is incorrect. Example: 380 50-123-45-67")]
        [DisplayFormat(DataFormatString = "{0:+38 XXX-XXX-XX-XX}")]
        public string DirectorPhonenumber { get; set; } = string.Empty;

        public string ManagerialBody { get; set; } = string.Empty;

        [Required]
        public OwnershipType Ownership { get; set; }

        [Required]
        public ProviderType Type { get; set; }

        public string Form { get; set; } = string.Empty;

        [Required]
        public ProviderProfile Profile { get; set; }

        [MaxLength(20)]
        public string Index { get; set; } = string.Empty;

        public bool IsSubmitPZ1 { get; set; } = default;

        [DataType(DataType.ImageUrl)]
        public string AttachedDocuments { get; set; } = string.Empty;

        [Required]
        public string AddressId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public virtual IEnumerable<Workshop> Workshops { get; }

        public virtual User User { get; set; }

        public virtual Address Address { get; set; }
    }
}