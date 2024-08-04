using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class Email : IKeyedEntity<long>
{
    public long Id { get; set; }

    [DataType(DataType.EmailAddress)]
    [MaxLength(256)]
    public string EmailAddress { get; set; }

    public string Type { get; set; }
}
