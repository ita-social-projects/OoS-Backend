namespace OutOfSchool.AuthCommon;

public static class AuthServerConstants
{
    public const string ExternalAuthSelectedRoleKey = "external_selected_role";
    public const string ExternalAuthUserIdKey = "external_user_id";

    public static class ClaimTypes
    {
        public const string UserId = "user_id";
        public const string Rnkopp = "rnokpp";
        public const string Edrpou = "edrpou";
    }
}