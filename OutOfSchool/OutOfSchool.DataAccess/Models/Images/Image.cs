using System;

namespace OutOfSchool.Services.Models.Images
{
    /// <summary>
    /// Encapsulates image data for some Entity.
    /// </summary>
    /// <typeparam name="TEntity">This is an entity for which you can operate with images.</typeparam>
    public class Image<TEntity>
    {
        public Guid EntityId { get; set; }

        public virtual TEntity Entity { get; set; }

        public string ExternalStorageId { get; set; }
    }
}
