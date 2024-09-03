﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;

public class WorkshopWithRequiredPropertiesDto
{
    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Website { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Facebook { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Instagram { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    [Required]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning FormOfLearning { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType? PayRate { get; set; } = PayRateType.Classes;

    public uint? AvailableSeats { get; set; } = uint.MaxValue;

    public bool CompetitiveSelection { get; set; }

    [MaxLength(500)]
    public string CompetitiveSelectionDescription { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; } = string.Empty;

    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitleEn { get; set; } = string.Empty;

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus ProviderLicenseStatus { get; set; } = ProviderLicenseStatus.NotProvided;

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