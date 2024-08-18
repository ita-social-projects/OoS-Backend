using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class Instagram : ContactEntityBase, IKeyedEntity<long>
{
    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string InstagramUrl { get; set; } = string.Empty;
}
