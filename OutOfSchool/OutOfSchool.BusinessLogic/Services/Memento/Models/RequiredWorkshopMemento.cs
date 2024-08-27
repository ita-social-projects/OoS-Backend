using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Services.Memento.Models;

public class RequiredWorkshopMemento
{
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; } = string.Empty;

    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; } = string.Empty;

    [EnumDataType(typeof(PayRateType))]
    public PayRateType PayRate { get; set; }

    public Guid ProviderId { get; set; }

    public long AddressId { get; set; }

    [EnumDataType(typeof(FormOfLearning))]
    public FormOfLearning FormOfLearning { get; set; }
}