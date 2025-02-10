namespace OutOfSchool.AuthCommon;

public static class AuthServerConstants
{
    public const string LoginPath = "login";
    public const string LogoutPath = "logout";
    public const string AllowedUserNameCharacters = "абвгдеєжзиіклмнопрстуфхцчшщюяАБВГДЕЄЖЗИІКЛМНОПРСТУФХЦЧШЩЮЯabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    public const string ExternalAuthSelectedRoleKey = "external_selected_role";
    public const string ExternalAuthUserIdKey = "external_user_id";

    public static class ClaimTypes
    {
        public const string UserId = "user_id";
        public const string Rnokpp = "rnokpp";
        public const string Edrpou = "edrpou";
    }

    public static class ExternalQuery
    {
        public const string UserId = "user_id";
        public const string AccessToken = "access_token";
        public const string Certificate = "cert";
    }

    public static class FeatureManagement
    {
        public const string PasswordLogin = "PasswordLogin";
        public const string PasswordRegistration = "PasswordRegistration";
        public const string EmailConfirmation = "EmailConfirmation";
        public const string EmailManagement = "EmailManagement";
        public const string PasswordManagement = "PasswordManagement";
    }
}