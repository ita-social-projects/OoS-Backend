namespace OutOfSchool.RazorTemplatesData.Services
{
    public static class RazorTemplates
    {
        /// <summary>
        /// Base path to the html email templates
        /// </summary>
        private static string MainPath => "/Views/Emails/";

        /// <summary>
        /// Base path to the plain text email templates
        /// </summary>
        private static string MainPathPlainText => "/ViewsPlainText/Emails/";

        // Supported email templates
        public static string ConfirmEmail => "ConfirmEmail";
        public static string ChangeEmail => "ChangeEmail";
        public static string ResetPassword => "ResetPassword";
        public static string NewAdminInvitation => "NewAdminInvitation";

        /// <summary>
        /// Get view name
        /// </summary>
        /// <param name="emailName">Email template name</param>
        /// <param name="isHtml">Indicates which template will be used: true = html, false - plain text.</param>
        /// <returns>View name</returns>
        internal static string GetViewName(string emailName, bool isHtml = true) 
            => $"{(isHtml ? MainPath : MainPathPlainText)}{emailName}/{emailName}.cshtml";

    }
}
