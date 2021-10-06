﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Provider
    {
        public long Id { get; set; }

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
        [MaxLength(256)]
        public string Website { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(256)]
        public string Facebook { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(256)]
        public string Instagram { get; set; } = string.Empty;

        [MaxLength(500)]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "EDRPOU/IPN code is required")]
        [RegularExpression(
            @"^(\d{8}|\d{10})$",
            ErrorMessage = "EDRPOU/IPN code must contain 8 or 10 digits")]
        [MaxLength(12)]
        public string EdrpouIpn { get; set; }

        [MaxLength(50)]
        public string Director { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime DirectorDateOfBirth { get; set; } = default;

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(
            @"([\d]{10})",
            ErrorMessage = "Phone number format is incorrect. Example: 0501234567")]
        [DisplayFormat(DataFormatString = "{0:+38 XXX-XXX-XX-XX}")]
        [MaxLength(15)]
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
    }
}