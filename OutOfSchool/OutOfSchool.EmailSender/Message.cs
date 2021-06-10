namespace OutOfSchool.EmailSender
{
    public class Message
    {
        internal EmailAddress To { get; set; }
        internal EmailAddress From { get; set; }
        internal string Subject { get; set; }
        internal string Content { get; set; }
    }
}
