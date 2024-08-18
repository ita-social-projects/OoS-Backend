using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class Facebook : ContactEntityBase, IKeyedEntity<long>
{
    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string FacebookUrl { get; set; } = string.Empty;
}
