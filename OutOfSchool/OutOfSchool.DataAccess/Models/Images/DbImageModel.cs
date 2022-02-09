using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models.Images
{
    public class DbImageModel
    {
        public Guid Id { get; set; }

        public byte[] File { get; set; }

        public string ContentType { get; set; }
    }
}
