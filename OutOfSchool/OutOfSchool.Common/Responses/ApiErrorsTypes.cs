using OutOfSchool.Common.Responces;

namespace OutOfSchool.Common.Responses;
public abstract class ApiErrorsTypes
{
    // Provider Admin, Create
    public abstract class ProviderAdmin
    {
        public abstract class Creation
        {
            public static ApiError UserDontHavePermission =>
            new ApiError(
                "1",
                "Current user doesn't have permission to create provider admin");
        }
    }
}
