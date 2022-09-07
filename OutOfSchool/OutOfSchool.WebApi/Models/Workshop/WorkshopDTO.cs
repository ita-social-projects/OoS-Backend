using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Util.CustomValidation;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models;

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

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    public bool WithDisabilityOptions { get; set; } = default;

    [MaxLength(200)]
    public string DisabilityOptionsDesc { get; set; } = string.Empty;

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }

    [Required]
    [MaxLength(60)]
    public string ProviderTitle { get; set; } = string.Empty;

    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<string> Keywords { get; set; } = default;

    [Required]
    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType PayRate { get; set; } = PayRateType.Classes;

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public uint TakenSeats { get; set; } = 0;

    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    public long AddressId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public string InstitutionHierarchy { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

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

    public List<DirectionDto> Directions { get; set; }

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