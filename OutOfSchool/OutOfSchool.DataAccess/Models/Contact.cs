using System;
using System.Collections.Generic;

namespace OutOfSchool.Services.Models;

public class Contact : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string UserNote { get; set; }

    public List<PhoneNumber> PhoneNumbers { get; set; }

    public OptionalContacts OptionalContacts { get; set; }
}
