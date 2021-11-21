using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class ImageStorageModelDto
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}
