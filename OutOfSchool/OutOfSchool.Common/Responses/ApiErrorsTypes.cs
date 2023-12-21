namespace OutOfSchool.Common.Responses;
public static class ApiErrorsTypes
{
    public static class Common
    {
        public static ApiError EmailAlreadyTaken(string entityName, string email) =>
        new ApiError(
            $"{nameof(Common)}",
            $"{nameof(EmailAlreadyTaken)}",
            $"{entityName} creating is not possible. Username {email} is already taken");
    }

    public static class ProviderAdmin
    {
        public static ApiError UserDontHavePermissionToCreate(string userId) =>
        new ApiError(
            $"{nameof(ProviderAdmin)}",
            $"{nameof(UserDontHavePermissionToCreate)}",
            $"User(id): {userId} doesn't have permission to create provider admin");
    }
}
