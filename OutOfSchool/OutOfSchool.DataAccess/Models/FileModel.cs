using System.IO;

namespace OutOfSchool.Services.Models
{
    public class FileModel
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}