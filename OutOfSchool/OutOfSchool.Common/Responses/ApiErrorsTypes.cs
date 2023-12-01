using System;
using System.Diagnostics;
using System.Reflection;
using OutOfSchool.Common.Responces;

namespace OutOfSchool.Common.Responses;
public abstract class ApiErrorsTypes
{
    private static string GetGroup()
    {
        StackTrace stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();
        MethodBase method;

        try
        {
            method = frames[1].GetMethod();
        }
        catch
        {
            return "undefined";
        }

        Type declaringType = method.DeclaringType;

        string subGroup = declaringType.Name;
        string group = declaringType.DeclaringType.Name;

        return $"{group}/{subGroup}";
    }

    public abstract class ProviderAdmin
    {
        private static int entityCode = 10;

        public abstract class Creation
        {
            private static int actionCode = 1;

            public static ApiError UserDontHavePermission(string userId) =>
            new ApiError(
                $"{entityCode}_{actionCode}",
                $"User(id): {userId} doesn't have permission to create provider admin",
                GetGroup());
        }

        // To be continued for another ProviderAdmin service actions
    }

    // To be continued for another entities
}
