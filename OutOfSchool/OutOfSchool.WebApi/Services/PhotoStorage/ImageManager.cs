using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace OutOfSchool.WebApi.Services.PhotoStorage
{
    public class ImageManager
    {
        private readonly Image image;

        public ImageManager(Image image, Size targetSize)
        {
            this.image = image;

            Origin = new Point(0, 0);

            TargetSize = targetSize;

            TargetRect = new Rectangle(Origin, TargetSize);
        }

        /// <summary>
        /// Gets or sets the target image size and position.
        /// </summary>
        private Rectangle TargetRect { get; set; }

        /// <summary>
        /// Gets or sets the target image size.
        /// </summary>
        private Size TargetSize { get; set; }

        /// <summary>
        /// Gets or sets the target image position.
        /// </summary>
        private Point Origin { get; set; }

        /// <summary>
        /// Returns byte array from image.
        /// </summary>
        /// <returns>Image in byte array.</returns>
        public byte[] GetImageInBytes()
        {
            var file = ResizeImage(image, TargetRect);

            using (var bitmap = file)
            {
                return BitmapToByteArray(bitmap);
            }
        }

        /// <summary>
        /// Resize the image.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="targetRect">The target image size and position.</param>
        /// <returns>Resized bitmap image.</returns>
        private Bitmap ResizeImage(Image image, Rectangle targetRect)
        {
            var destImage = new Bitmap(targetRect.Width, targetRect.Height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;

                graphics.DrawImage(image, 0, 0, targetRect.Width, targetRect.Height);
            }

            return destImage;
        }

        /// <summary>
        /// Converts bitmap image into byte array.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <returns>Byte array.</returns>
        private byte[] BitmapToByteArray(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                long quality = 75;
                var countEncoderParameters = 1;
                var qualityParamId = Encoder.Quality;
                var encoderParameters = new EncoderParameters(countEncoderParameters);
                encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
                var codec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                image.Save(stream, codec, encoderParameters);

                encoderParameters.Dispose();

                return stream.ToArray();
            }
        }
    }
}
