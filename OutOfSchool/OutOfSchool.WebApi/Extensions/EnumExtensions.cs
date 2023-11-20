using System.Reflection;

namespace OutOfSchool.WebApi.Extensions;

public static class EnumExtensions
{
    public static T GetAttribute<T>(this Enum value)
        where T : Attribute
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString()).First();
        var attribute = memberInfo.GetCustomAttribute(typeof(T), false);
        return (T)attribute;
    }
}