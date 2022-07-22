namespace OutOfSchool.Common;

public static class Constants
{
    public const int UnifiedUrlLength = 256;

    public const string BearerScheme = "Bearer";

    public const string PhoneNumberFormat = "{0:+380 XX-XXX-XX-XX}";

    public const string PhoneNumberRegexViewModel = @"([0-9]{2})([-]?)([0-9]{3})([-]?)([0-9]{2})([-]?)([0-9]{2})";

    public const string PhoneNumberRegexModel = @"([\d]{9})";

    public const string PhoneErrorMessage = "Error! Please check the number is correct";

    public const int UnifiedPhoneLength = 15;

    public const int MySQLServerMinimalMajorVersion = 8;

    public const string NameRegexViewModel = @"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$";

    public const string NameErrorMessage = "Check the entered data. Please use only cyrillic and symbols( ' - )";

    public const string EmailRegexViewModel = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

    public const string PasswordRegexViewModel = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";

    public const string AddressSeparator = ", ";

    public const string EnumErrorMessage = "{0} should be in enum range";

    public const string AdminKeyword = "Admin";
}