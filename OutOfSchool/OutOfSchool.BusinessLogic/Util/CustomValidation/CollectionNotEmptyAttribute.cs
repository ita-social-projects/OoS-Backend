using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Property)]
public class CollectionNotEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value switch
        {
            ICollection collection => collection.Count > 0,
            IEnumerable enumerable => enumerable.GetEnumerator().MoveNext(),
            _ => false
        };
    }
}