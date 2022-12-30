using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class ProviderTypeDto
{
    public long Id { get; set; }

    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Id} {Name}";
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = (hash * 7) + Id.GetHashCode();
            hash = (hash * 7) + (!ReferenceEquals(null, Name) ? Name.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not ProviderTypeDto type)
        {
            return false;
        }

        return Id == type.Id &&
               string.Equals(Name, type.Name, StringComparison.OrdinalIgnoreCase);
    }
}