using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class Email : ContactEntityBase, IKeyedEntity<long>
{
    [DataType(DataType.EmailAddress)]
    [MaxLength(256)]
    public string EmailAddress { get; set; }
}
