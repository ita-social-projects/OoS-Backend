using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OutOfSchool.Services.Models.Images
{
    public class ExternalImageModel
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}
