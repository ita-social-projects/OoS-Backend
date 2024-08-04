using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class ContactInformation : IKeyedEntity<long>
{
    public long Id { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Facebook { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Instagram { get; set; } = string.Empty;

    public List<Address> Addresses { get; set; }

    public List<PhoneNumber> PhoneNumbers { get; set; }

    public List<Email> Emails { get; set; }

    public List<Website> Websites { get; set; }
}
