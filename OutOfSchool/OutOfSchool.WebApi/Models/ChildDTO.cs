using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Attributes.Validation;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(30, MinimumLength = 1)]
        [NameIsAllowed(ErrorMessage = "First name contains invalid characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(30, MinimumLength = 1)]
        [NameIsAllowed(ErrorMessage = "Last name contains invalid characters")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(30, MinimumLength = 1)]
        [NameIsAllowed(ErrorMessage = "Middle name contains invalid characters")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;

        [Required(ErrorMessage = "Gender is required")]
        [Range(0, 1)]
        public Gender Gender { get; set; } = default;

        [MaxLength(500)]
        public string PlaceOfStudy { get; set; } = string.Empty;

        public long ParentId { get; set; } = default;

        public long? SocialGroupId { get; set; } = default;

        // TODO: define if we really need this in dto
        [JsonIgnore]
        public ParentDTO Parent { get; set; }
    }
}