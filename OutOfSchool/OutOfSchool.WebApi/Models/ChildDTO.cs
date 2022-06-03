using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(30, MinimumLength = 1)]
        [RegularExpression(@"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$", ErrorMessage = "First name contains invalid characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(30, MinimumLength = 1)]
        [RegularExpression(@"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$", ErrorMessage = "Last name contains invalid characters")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(30, MinimumLength = 1)]
        [RegularExpression(@"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$", ErrorMessage = "Middle name contains invalid characters")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;

        [Required(ErrorMessage = "Gender is required")]
        [Range(0, 1)]
        public Gender Gender { get; set; } = default;

        [MaxLength(500)]
        public string PlaceOfStudy { get; set; } = string.Empty;

        public Guid ParentId { get; set; } = default;

        public List<SocialGroupDto> SocialGroups { get; set; }

        public ParentDtoWithContactInfo Parent{ get; set; }
    }
}