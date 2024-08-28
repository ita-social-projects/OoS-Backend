using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class SocialNetworkContact
{
    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Url { get; set; } = string.Empty;

    public SocialNetworkContactType Type { get; set; }
}
