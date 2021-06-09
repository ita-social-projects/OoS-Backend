﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OutOfSchool.Services.Models
{
    public class Workshop
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Workshop title is required")]
        [MinLength(1)]
        [MaxLength(60)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            @"([\d]{10})",
            ErrorMessage = "Phone number format is incorrect. Example: 0501234567")]
        [DisplayFormat(DataFormatString = "{0:+38 XXX-XXX-XX-XX}")]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        public string Website { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Facebook { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Instagram { get; set; } = string.Empty;

        [Required(ErrorMessage = "Children's min age is required")]
        [Range(0, 18, ErrorMessage = "Min age should be a number from 0 to 18")]
        public int MinAge { get; set; }

        [Required(ErrorMessage = "Children's max age is required")]
        [Range(0, 18, ErrorMessage = "Max age should be a number from 0 to 18")]
        public int MaxAge { get; set; }

        [Required(ErrorMessage = "Specify how many times per week lessons will be held")]
        [Range(1, 7, ErrorMessage = "Field should be a digit from 1 to 7")]
        public int DaysPerWeek { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000, ErrorMessage = "Field value should be in a range from 1 to 10 000")]
        public decimal Price { get; set; } = default;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool WithDisabilityOptions { get; set; } = default;

        [MaxLength(200)]
        public string DisabilityOptionsDesc { get; set; } = string.Empty;

        public string Logo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Head's information is required")]
        [MaxLength(50, ErrorMessage = "Field should not be longer than 50 characters")]
        public string Head { get; set; } = string.Empty;

        [Required(ErrorMessage = "Head's date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime HeadDateOfBirth { get; set; }

        [Required]
        public bool IsPerMonth { get; set; }

        [Required]
        public long ProviderId { get; set; }

        [Required]
        public long AddressId { get; set; }

        [Required]
        public long CategoryId { get; set; }

        [Required]
        public long SubcategoryId { get; set; }

        [Required]
        public long SubsubcategoryId { get; set; }

        public virtual Provider Provider { get; set; }

        public virtual Address Address { get; set; }

        public virtual Subsubcategory Subsubcategory { get; set; }

        public virtual List<Teacher> Teachers { get; set; }
    }
}