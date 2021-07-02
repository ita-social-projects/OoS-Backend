namespace OutOfSchool.EmailSender
{
    public class Message
    {
        public EmailAddress To { get; set; }
        public EmailAddress From { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
