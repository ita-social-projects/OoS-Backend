using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class Facebook : IKeyedEntity<long>
{
    public long Id { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Url { get; set; } = string.Empty;

    public string Type { get; set; }
}
