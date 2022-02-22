using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Services.Models.Images
{
    public class DbImageModel
    {
        public Guid Id { get; set; }

        public byte[] File { get; set; }

        [MaxLength(20)]
        public string ContentType { get; set; }
    }
}
