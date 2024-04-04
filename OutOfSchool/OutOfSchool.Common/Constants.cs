namespace OutOfSchool.Common;

public static class Constants
{
    public const int UnifiedUrlLength = 256;

    public const long DefaultCityCodeficatorId = 31737;

    public const string DefaultAuthScheme = "bearer_or_cookie";

    public const string PhoneNumberFormat = "{0:XXXX XX-XXX-XX-XX}";

    public const string PhoneErrorMessage = "Error! Please check the number is correct";

    public const int MySQLServerMinimalMajorVersion = 8;

    public const string NameRegexViewModel = @"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$";

    public const string NameErrorMessage = "Check the entered data. Please use only cyrillic and symbols( ' - )";

    public const string EmailRegexViewModel = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

    public const string PasswordValidationErrorMessage = "Password must be at least 8 characters long, including upper and lower case letters, digits and special characters (@$!%*?&)";

    public const string PasswordRequiredErrorMessage = "Password is required";

    public const int PasswordMinLength = 8;

    public const string ValidationSymbols = "@$!%*?&";

    public const string AddressSeparator = ", ";

    public const string EnumErrorMessage = "{0} should be in enum range";

    public const string AdminKeyword = "Admin";

    public const char MappingSeparator = '¤';

    public const int NameMaxLength = 60;

    public const int ChatMessageTextMaxLength = 256;

    public const string CacheProfilePrivate = "CacheProfilePrivate";

    public const string CacheProfilePublic = "CacheProfilePublic";

    public const string PathToChatHub = "/hubs/chat";

    public const string PathToNotificationHub = "/hubs/notification";

    /// <summary>
    /// Longest possible length of provider founder.
    /// </summary>
    public const int MaxProviderFounderLength = 60;

    /// <summary>
    /// Shortest possible phone number length without '+' prefix.
    /// </summary>
    public const int MinPhoneNumberLength = 7;

    /// <summary>
    /// Longest possible phone number length without '+' prefix.
    /// </summary>
    public const int MaxPhoneNumberLength = 15;

    /// <summary>
    /// Shortest possible phone number length with '+' prefix.
    /// </summary>
    public const int MinPhoneNumberLengthWithPlusSign = MinPhoneNumberLength + 1;

    /// <summary>
    /// Longest possible phone number length with '+' prefix.
    /// </summary>
    public const int MaxPhoneNumberLengthWithPlusSign = MaxPhoneNumberLength + 1;
}