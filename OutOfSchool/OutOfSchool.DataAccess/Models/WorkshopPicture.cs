using System;

namespace OutOfSchool.Services.Models
{
    public class WorkshopPicture
    {
        public long WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public Guid PictureId { get; set; }

        public virtual WorkshopPictureMetadata Picture { get; set; }
    }
}
