namespace OutOfSchool.EmailSender
{
    internal class SmtpConfiguration
    {
        internal string Server { get; set; }
        internal int Port { get; set; }
        internal string Username { get; set; }
        internal string Password { get; set; }
    }
}
