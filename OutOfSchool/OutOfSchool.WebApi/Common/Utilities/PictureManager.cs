using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace OutOfSchool.WebApi.Common.Utilities
{
    public class PictureManager
    {
        private const long Quality = 75;
        private const int CountEncoderParameters = 1;

        private readonly Image image;

        public PictureManager(Image image, Size targetSize)
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
        /// Returns stream from image.
        /// </summary>
        /// <returns>Stream.</returns>
        public Stream GetStreamFromImage()
        {
            var file = ResizeImage(image, TargetRect);

            using (var bitmap = file)
            {
                return BitmapToStream(file);
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

                graphics.DrawImage(image, uint.MinValue, uint.MinValue, targetRect.Width, targetRect.Height);
            }

            return destImage;
        }

        /// <summary>
        /// Converts bitmap image into stream.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <returns>Stream.</returns>
        private Stream BitmapToStream(Bitmap image)
        {
            var stream = new MemoryStream();
            var qualityParamId = Encoder.Quality;
            var encoderParameters = new EncoderParameters(CountEncoderParameters);
            encoderParameters.Param[uint.MinValue] = new EncoderParameter(qualityParamId, Quality);
            var codec = ImageCodecInfo.GetImageDecoders()
                .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

            image.Save(stream, codec, encoderParameters);

            encoderParameters.Dispose();

            return stream;
        }
    }
}
