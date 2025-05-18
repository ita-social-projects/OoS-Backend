using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;

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
            { typeof(Image<WorkshopDraft>), ProjectWorkshopDraftImage }
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

    /// <summary>
    /// Extracts the external storage ID from a <see cref="Image{WorkshopDraft}"/> object.
    /// Used to convert image references into a comparable string format for logging purposes.
    /// </summary>
    /// <param name="obj">The object to extract the image ID from.</param>
    /// <returns>The external storage ID if the object is a valid image; otherwise, <c>null</c>.</returns>
    private static string ProjectWorkshopDraftImage(object obj)
        => obj is Image<WorkshopDraft> image
            ? image.ExternalStorageId
            : null;
}