namespace OutOfSchool.Services.Contexts.Configuration
{
    /// <summary>
    /// Encapsulates connecting options for an external image storage.
    /// </summary>
    public class ExternalImageSourceConfig
    {
        public const string Name = "ConnectionStrings:ExternalImageStorage";

        public string ServerName { get; set; }

        public string DatabaseName { get; set; }
    }
}
