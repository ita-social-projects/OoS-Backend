using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class SocialNetworkContactDto
{
    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Url { get; set; } = string.Empty;

    public SocialNetworkContactType Type { get; set; }
}