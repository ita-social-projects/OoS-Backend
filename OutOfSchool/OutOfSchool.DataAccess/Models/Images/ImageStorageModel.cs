using System.IO;

namespace OutOfSchool.Services.Models.Images
{
    /// <summary>
    /// Represents image model with Stream and ContentType.
    /// </summary>
    public class ImageStorageModel
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}
