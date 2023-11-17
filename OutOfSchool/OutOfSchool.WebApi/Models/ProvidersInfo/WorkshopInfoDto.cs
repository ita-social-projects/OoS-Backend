using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Util.CustomValidation;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models.ProvidersInfo;

public class WorkshopInfoDto : WorkshopInfoBaseDto
{
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
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType? PayRate { get; set; } = PayRateType.Classes;

    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public bool CompetitiveSelection { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    public bool WithDisabilityOptions { get; set; } = default;

    [MaxLength(200)]
    public string DisabilityOptionsDesc { get; set; } = string.Empty;

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public string InstitutionHierarchy { get; set; }

    public List<long> DirectionIds { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<string> Keywords { get; set; } = default;

    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public AddressDto Address { get; set; }

    public List<TeacherDTO> Teachers { get; set; }

    public uint TakenSeats { get; set; } = 0;

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

}
