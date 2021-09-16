using System;

namespace OutOfSchool.Services.Models
{
    public class TeacherPicture
    {
        public long TeacherId { get; set; }

        public virtual Teacher Teacher { get; set; }

        public Guid PictureId { get; set; }

        public virtual PictureMetadata Picture { get; set; }
    }
}
