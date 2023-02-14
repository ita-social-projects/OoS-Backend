using OutOfSchool.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderDto : ProviderBaseDto
{
    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }
}