using System;

namespace OutOfSchool.Services.Models.Images
{
    /// <summary>
    /// Encapsulates image data for some Entity.
    /// </summary>
    /// <typeparam name="TEntity">This is an entity for which you can operate with images.</typeparam>
    public class Image<TEntity>
    {
        public Guid Id { get; set; }

        public virtual TEntity Entity { get; set; }

        public Guid ImageId { get; set; }

        public virtual ImageMetadata ImageMetadata { get; set; }
    }
}
