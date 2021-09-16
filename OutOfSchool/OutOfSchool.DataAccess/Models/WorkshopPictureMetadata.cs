using System;

namespace OutOfSchool.Services.Models
{
    public class WorkshopPictureMetadata
    {
        public Guid Id { get; set; }

        public virtual WorkshopPicture WorkshopPicture { get; set; }

        public string StorageId { get; set; }

        public string ContentType { get; set; }
    }
}
