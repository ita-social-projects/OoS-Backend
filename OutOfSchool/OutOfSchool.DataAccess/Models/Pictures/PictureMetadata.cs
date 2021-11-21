using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models.Pictures
{
    public class PictureMetadata
    {
        public Guid Id { get; set; }

        public string StorageId { get; set; }

        public virtual Picture<Workshop> WorkshopPicture { get; set; }

        public string ContentType { get; set; }
    }
}
