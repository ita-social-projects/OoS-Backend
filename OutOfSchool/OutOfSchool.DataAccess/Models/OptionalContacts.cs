using System.Collections.Generic;

namespace OutOfSchool.Services.Models;

/// <summary>
/// This is just a helper class, not a real DB entity.
/// </summary>
public class OptionalContacts
{
    public List<Facebook> Facebooks { get; set; }

    public List<Instagram> Instagrams { get; set; }

    public List<Email> Emails { get; set; }

    public List<Website> Websites { get; set; }
}
