using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OutOfSchool.Services.Models.Pictures
{
    public class PictureStorageModel
    {
        public Stream ContentStream { get; set; }

        public string ContentType { get; set; }
    }
}
