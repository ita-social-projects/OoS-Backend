using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Common
{
    public static class Constants
    {
        public const int UnifiedUrlLength = 256;

        public const string SectionName = "FeatureManagement";

        public const string PhoneNumberFormat = "{0:+380 XX-XXX-XX-XX}";

        public const string PhoneNumberRegexViewModel = @"([0 - 9]{2})([-]?) ([0 - 9]{3})([-] ?)([0 - 9]{ 2})([-] ?)([0 - 9]{ 2})";

        public const string PhoneNumberRegexModel = @"([\d]{9})";

        public const string PhoneErrorMessage = "Error! Please check the number is correct";

        public const int UnifiedPhoneLength = 15;

        public const int MySQLServerMinimalMajorVersion = 8;
    }
}