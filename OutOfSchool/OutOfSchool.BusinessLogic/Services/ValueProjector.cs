using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Services;

public class ValueProjector : IValueProjector
{
    private readonly Dictionary<Type, Func<object, string>> projectors;

    public ValueProjector()
    {
        projectors = new Dictionary<Type, Func<object, string>>
        {
            { typeof(ContactsAddress), ProjectAddress },
            { typeof(Institution), ProjectInstitution },
        };
    }

    public string ProjectValue(Type type, object value)
    {
        if (value == null)
        {
            return null;
        }

        var projector = GetValueProjector(type);
        if (projector != null)
        {
            return projector.Invoke(value);
        }

        return value.ToString();
    }

    private Func<object, string> GetValueProjector(Type type)
    {
        if (projectors.TryGetValue(type, out var projector))
        {
            return projector;
        }

        return null;
    }

    private string ProjectAddress(object obj)
    {
        if (obj is ContactsAddress address)
        {
            return $"{address.CATOTTGId}, {address.Street}, {address.BuildingNumber}";
        }
        else if (obj is List<Contacts> contacts)
        {
            var defaultContact = contacts.FirstOrDefault(c => c.IsDefault);
            return defaultContact?.Address != null 
                ? ProjectAddress(defaultContact.Address)
                : null;
        }
        
        return null;
    }

    private string ProjectInstitution(object obj) => obj is Institution institution
        ? institution.Title
        : null;
}