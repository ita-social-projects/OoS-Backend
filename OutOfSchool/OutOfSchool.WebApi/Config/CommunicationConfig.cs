namespace OutOfSchool.WebApi.Config
{
    public class CommunicationConfig
    {
        public const string Name = "Communication";

        public int TimeoutInSeconds { get; set; }

        public int MaxNumberOfRetries { get; set; }

        public string ClientName { get; set; }
    }
}