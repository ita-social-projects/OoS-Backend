using System;
using System.Collections;

namespace OutOfSchool.WebApi.Util.CustomValidation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionNotEmptyAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
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
}