#nullable enable

using System.Collections.Generic;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.Services.Models;

public interface IHasContacts
{
    // Owned "Contact" data
    public List<Contacts> Contacts { get; set; }
}