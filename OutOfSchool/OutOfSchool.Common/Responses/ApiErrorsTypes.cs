using OutOfSchool.Common.Responces;

namespace OutOfSchool.Common.Responses;
public abstract class ApiErrorsTypes
{
    public abstract class ProviderAdmin
    {
        public abstract class Creation
        {
            public static ApiError UserDontHavePermission(string userId) =>
            new ApiError(
                "1", // It will be another id later
                $"User(id): {userId} doesn't have permission to create provider admin");
        }

        // To be continued for another ProviderAdmin service actions
    }

    // To be continued for another entities
}
