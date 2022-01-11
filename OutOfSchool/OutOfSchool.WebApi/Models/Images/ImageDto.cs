using System.IO;

namespace OutOfSchool.WebApi.Models.Images
{
    public class ImageDto
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}
