using System.Collections.Generic;

namespace OutOfSchool.Services.Models;

public class ContactInformation : IKeyedEntity<long>
{
    public long Id { get; set; }

    public List<Facebook> Facebooks { get; set; }

    public List<Instagram> Instagrams { get; set; }

    public List<Address> Addresses { get; set; }

    public List<PhoneNumber> PhoneNumbers { get; set; }

    public List<Email> Emails { get; set; }

    public List<Website> Websites { get; set; }
}
