using System;

namespace OutOfSchool.Services.Models.Images
{
    /// <summary>
    /// Encapsulates image metadata which contains external Storage Id and ContentType.
    /// </summary>
    public class ImageMetadata
    {
        public Guid Id { get; set; }

        public string StorageId { get; set; }

        public virtual Image<Workshop> WorkshopImage { get; set; }

        public string ContentType { get; set; }
    }
}
