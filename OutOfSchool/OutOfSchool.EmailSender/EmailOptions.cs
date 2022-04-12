namespace OutOfSchool.EmailSender
{
    public class EmailOptions
    {
        public static string SectionName => "Email";
        public string AddressFrom { get; set; }
        public string NameFrom { get; set; }
        public string SendGridKey { get; set; }
        public bool Enabled { get; set; }
    }
}
