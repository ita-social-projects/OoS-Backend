using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace OutOfSchool.WebApi.Config.Pictures
{
    public class PictureOptions<TEntity>
    {
        public ushort MinWidthPixels { get; set; }

        public ushort MaxWidthPixels { get; set; }

        public ushort MinHeightPixels { get; set; }

        public ushort MaxHeightPixels { get; set; }

        public int MaxSizeBytes { get; set; }

        public List<string> SupportedFormats { get; set; }
    }
}
