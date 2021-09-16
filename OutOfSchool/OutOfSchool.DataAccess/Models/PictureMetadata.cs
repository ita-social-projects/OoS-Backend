using System;

namespace OutOfSchool.Services.Models
{
    public class PictureMetadata
    {
        public Guid Id { get; set; }

        public string StorageId { get; set; }

        public string ContentType { get; set; }

        public virtual WorkshopPicture WorkshopPicture { get; set; }

        public virtual ProviderPicture ProviderPicture { get; set; }

        public virtual TeacherPicture TeacherPicture { get; set; }
    }
}
