using System.Collections.Generic;

namespace OutOfSchool.Services.Models;

public class ContactInformation : IKeyedEntity<long>
{
    public long Id { get; set; }

    public List<Address> Addresses { get; set; }
}
