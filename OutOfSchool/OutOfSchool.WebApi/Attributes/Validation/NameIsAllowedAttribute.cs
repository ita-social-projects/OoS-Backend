using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class NameIsAllowedAttribute : ValidationAttribute
    {
        public const string Pattern = @"^[А-ЩЬЮЯҐЄІЇ](([\'\-][А-ЩЬЮЯҐЄІЇа-щьюяґєії])?[а-щьюяґєії]*)*$";

        public override bool IsValid(object value)
        {
            if (value is string name)
            {
                return Regex.IsMatch(name, Pattern);
            }

            return false;
        }
    }
}
