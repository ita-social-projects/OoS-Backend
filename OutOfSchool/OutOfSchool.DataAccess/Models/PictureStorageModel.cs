using System.IO;

namespace OutOfSchool.Services.Models
{
    public class PictureStorageModel
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}
