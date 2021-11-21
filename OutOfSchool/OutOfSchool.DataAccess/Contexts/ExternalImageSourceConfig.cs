namespace OutOfSchool.Services.Contexts
{
    /// <summary>
    /// Encapsulates connecting options for an external image storage.
    /// </summary>
    public class ExternalImageSourceConfig
    {
        public const string Name = "ConnectionStrings:ExternalImageStorage";

        public string Server { get; set; }

        public string Database { get; set; }
    }
}
