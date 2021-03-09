#nullable enable
using System;
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
        public string Title { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            @"([\d]{9})",
            ErrorMessage = "Phone number format is incorrect. Example: 380 50-123-45-67")]
        [DisplayFormat(DataFormatString = "{0:+38 XXX-XXX-XX-XX}")]
        public string Phone { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [DataType(DataType.Url)]
        public string? Website { get; set; }

        [MaxLength(30)]
        public string? Facebook { get; set; }

        [MaxLength(30)]
        public string? Instagram { get; set; }

        [Required(ErrorMessage = "Children's min age is required")]
        [Range(0, 16, ErrorMessage = "Min age should be a number from 0 to 16")]
        public int MinAge { get; set; }

        [Required(ErrorMessage = "Children's max age is required")]
        [Range(0, 16, ErrorMessage = "Max age should be a number from 0 to 16")]
        public int MaxAge { get; set; }

        [Required(ErrorMessage = "Specify how many times per week lessons will be held")]
        [Range(1, 7, ErrorMessage = "Field should be a digit from 1 to 7")]
        public int DaysPerWeek { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        [Range(1, 10000, ErrorMessage = "Field value should be in a range from 1 to 10 000")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [RegularExpression(@"(^\d+(,\d{1,2})?$)")]
        [MaxLength(500)]
        public string Description { get; set; }

        public bool WithDisabilityOptions { get; set; }

        [MaxLength(200)]
        public string? DisabilityOptionsDesc { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Head's information is required")]
        [MaxLength(50, ErrorMessage = "Field should not be longer than 50 characters")]
        public string Head { get; set; }

        [Required(ErrorMessage = "Head's birthday is required")]
        [DataType(DataType.Date)]
        public DateTime HeadBirthDate { get; set; }

        public virtual Provider Provider { get; set; }

        public virtual Address Address { get; set; }

        public virtual Category Category { get; set; }

        public virtual List<Teacher> Teachers { get; set; }
    }
}