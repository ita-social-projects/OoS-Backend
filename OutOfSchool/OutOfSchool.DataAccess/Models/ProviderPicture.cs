using System;

namespace OutOfSchool.Services.Models
{
    public class ProviderPicture
    {
        public long ProviderId { get; set; }

        public virtual Provider Provider { get; set; }

        public Guid PictureId { get; set; }

        public virtual PictureMetadata Picture { get; set; }
    }
}
