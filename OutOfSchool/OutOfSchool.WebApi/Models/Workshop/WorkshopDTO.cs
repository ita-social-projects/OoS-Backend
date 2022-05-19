using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models
{
    public class WorkshopDTO : IValidatableObject
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Workshop title is required")]
        [MinLength(1)]
        [MaxLength(60)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            Constants.PhoneNumberRegexModel,
            ErrorMessage = Constants.PhoneErrorMessage)]
        [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
        [MaxLength(Constants.UnifiedPhoneLength)]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Website { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Facebook { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Instagram { get; set; } = string.Empty;

        [Required(ErrorMessage = "Children's min age is required")]
        [Range(0, 18, ErrorMessage = "Min age should be a number from 0 to 18")]
        public int MinAge { get; set; }

        [Required(ErrorMessage = "Children's max age is required")]
        [Range(0, 18, ErrorMessage = "Max age should be a number from 0 to 18")]
        public int MaxAge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000, ErrorMessage = "Field value should be in a range from 1 to 10 000")]
        public decimal Price { get; set; } = default;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool WithDisabilityOptions { get; set; } = default;

        [MaxLength(200)]
        public string DisabilityOptionsDesc { get; set; } = string.Empty;

        [MaxLength(256)]
        public string CoverImageId { get; set; } = string.Empty;

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Head's information is required")]
        [MaxLength(50, ErrorMessage = "Field should not be longer than 50 characters")]
        public string Head { get; set; } = string.Empty;

        [Required(ErrorMessage = "Head's date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime HeadDateOfBirth { get; set; }

        [Required]
        [MaxLength(60)]
        public string ProviderTitle { get; set; } = string.Empty;

        public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public IEnumerable<string> Keywords { get; set; } = default;

        [Required]
        public bool IsPerMonth { get; set; }

        public float Rating { get; set; }

        public int NumberOfRatings { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public long AddressId { get; set; }

        [Required]
        public long DirectionId { get; set; }

        public string Direction { get; set; }

        [Required]
        public long DepartmentId { get; set; }

        [Required]
        public long ClassId { get; set; }

        [Required]
        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public AddressDto Address { get; set; }

        public List<TeacherDTO> Teachers { get; set; }

        [Required]
        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public List<DateTimeRangeDto> DateTimeRanges { get; set; }

        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public IList<string> ImageIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<IFormFile> ImageFiles { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // TODO: Validate DateTimeRanges are not empty when frontend is ready
            foreach (var dateTimeRange in DateTimeRanges)
            {
                if (dateTimeRange.StartTime > dateTimeRange.EndTime)
                {
                    yield return new ValidationResult(
                        "End date can't be earlier that start date");
                }

                if (dateTimeRange.Workdays.IsNullOrEmpty() || dateTimeRange.Workdays.Any(workday => workday == DaysBitMask.None))
                {
                    yield return new ValidationResult(
                        "Workdays are required");
                }

                var daysHs = new HashSet<DaysBitMask>();
                if (!dateTimeRange.Workdays.All(daysHs.Add))
                {
                    yield return new ValidationResult(
                        "Workdays contain duplications");
                }
            }
        }
    }
}