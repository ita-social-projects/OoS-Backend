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

        public static ApiError PhoneNumberAlreadyTaken(string entityName, string phoneNumber) =>
            new ApiError(
                $"{nameof(Common)}",
                $"{nameof(PhoneNumberAlreadyTaken)}",
                $"{entityName} creating is not possible. Phone number {phoneNumber} is already taken");

        public static ApiError EntityIdDoesNotExist(string entityName, string id) =>
            new ApiError(
                $"{nameof(Common)}",
                $"{nameof(EntityIdDoesNotExist)}",
                $"{entityName} with id - {id} does not exist.");
    }

    public static class Employee
    {
        public static ApiError UserDontHavePermissionToCreate(string userId) =>
            new ApiError(
                $"{nameof(Employee)}",
                $"{nameof(UserDontHavePermissionToCreate)}",
                $"User(id): {userId} doesn't have permission to create employee");
    }

    public static class Application
    {
        public static ApiError AcceptRejectedWorkshopIsFull() =>
            new ApiError(
                $"{nameof(Application)}",
                $"{nameof(AcceptRejectedWorkshopIsFull)}",
                $"The Workshop is full.");

        public static ApiError AcceptRejectedAlreadyApproved() =>
            new ApiError(
                $"{nameof(Application)}",
                $"{nameof(AcceptRejectedAlreadyApproved)}",
                $"There is already approved workshop.");
    }
}
