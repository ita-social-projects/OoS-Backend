using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public abstract class ContactEntityBase : IKeyedEntity<long>
{
    public long Id { get; set; }

    public ContactInformationType Type { get; set; }
}
