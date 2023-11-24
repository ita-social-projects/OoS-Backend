namespace OutOfSchool.WebApi.Services;

public class ValueProjector : IValueProjector
{
    private readonly Dictionary<Type, Func<object, string>> projectors;

    public ValueProjector()
    {
        projectors = new Dictionary<Type, Func<object, string>>
        {
            { typeof(Address), ProjectAddress },
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

    private string ProjectAddress(object obj) => obj is Address address
        ? $"{address.CATOTTGId}, {address.Street}, {address.BuildingNumber}"
        : null;

    private string ProjectInstitution(object obj) => obj is Institution institution
        ? institution.Title
        : null;
}