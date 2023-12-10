using OutOfSchool.Common.Responces;

namespace OutOfSchool.Common.Responses;
public static class ApiErrorsTypes
{
    public static class ProviderAdmin
    {
        public static ApiError UserDontHavePermissionToCreate(string userId) =>
        new ApiError(
            $"{nameof(UserDontHavePermissionToCreate)}",
            $"User(id): {userId} doesn't have permission to create provider admin",
            $"{nameof(ProviderAdmin)}");
    }
}
