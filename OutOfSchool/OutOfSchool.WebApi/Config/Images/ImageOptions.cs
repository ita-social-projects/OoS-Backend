using System.Collections.Generic;

namespace OutOfSchool.WebApi.Config.Images
{
    /// <summary>
    /// Describes image options for some Entity.
    /// </summary>
    /// <typeparam name="TEntity">This is an entity for which you can operate with images.</typeparam>
    public class ImageOptions<TEntity>
    {
        public int MinWidthPixels { get; set; }

        public int MaxWidthPixels { get; set; }

        public int MinHeightPixels { get; set; }

        public int MaxHeightPixels { get; set; }

        public int MaxWidthHeightRatio { get; set; }

        public int MaxSizeBytes { get; set; }

        public List<string> SupportedFormats { get; set; }
    }
}
