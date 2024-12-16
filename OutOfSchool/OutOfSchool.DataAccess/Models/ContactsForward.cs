using System;

namespace OutOfSchool.Services.Models;

public class ContactsForward: IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }
    public string AddressType { get; set; }
    public Address Address { get; set; }
    public string Website { get; set; }
    public string Facebook { get; set; }
    public string Instagram { get; set; }
    public Contact Contact { get; set; }
}