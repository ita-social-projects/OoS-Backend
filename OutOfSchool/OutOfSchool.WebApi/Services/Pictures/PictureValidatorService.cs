using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Common.Exceptions.Pictures;
using OutOfSchool.WebApi.Config.Pictures;
using SharpCompress.Common;

namespace OutOfSchool.WebApi.Services.Pictures
{
    public class PictureValidatorService<TEntity> : IPictureValidatorService<TEntity>
    {
        private readonly PictureOptions<TEntity> options;

        public PictureValidatorService(IOptions<PictureOptions<TEntity>> options)
        {
            this.options = options.Value;
        }

        public bool ValidateImageSize(long size)
        {
            return size <= options.MaxSizeBytes;
        }

        public bool ValidateImageResolution(int width, int height)
        {
            return width >= options.MinWidthPixels
                   || width <= options.MaxWidthPixels
                   || height >= options.MinHeightPixels
                   || height <= options.MaxHeightPixels;
        }

        public bool ValidateImageFormat(string format)
        {
            return options.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
        }
    }
}
