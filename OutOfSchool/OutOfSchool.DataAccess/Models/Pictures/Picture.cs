using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models.Pictures
{
    public class Picture<TPictureEntity>
    {
        public Guid Id { get; set; }

        public virtual TPictureEntity Entity { get; set; }

        public Guid PictureId { get; set; }

        public virtual PictureMetadata PictureMetadata { get; set; }
    }
}
