using System.Drawing;
using System.Drawing.Drawing2D;

namespace OutOfSchool.WebApi.Services.PhotoStorage
{
    public class Scale
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scale"/> class.
        /// Scale an image as per given size and keep aspect ratio.
        /// The final result of the scale may have different width or hight.
        /// </summary>
        /// <param name="imgSize">Image size.</param>
        /// <param name="targetSize">Target size.</param>
        public Scale(Size imgSize, Size targetSize)
        {
            ImgSize = imgSize;
            Origin = new Point(0, 0);

            TargetSize
                = targetSize.Height == 0 && targetSize.Width > 0 ? ScaleByWidth(imgSize, targetSize.Width)
                : targetSize.Width == 0 && targetSize.Height > 0 ? ScaleByHeight(imgSize, targetSize.Height)
                : AutoScale(imgSize, targetSize);
        }

        /// <summary>
        /// Gets the source reading rectangle from the source image.
        /// </summary>
        public Rectangle SourceRect => new Rectangle(Origin, ImgSize);

        /// <summary>
        /// Gets the target image size and position.
        /// </summary>
        public Rectangle TargetRect => new Rectangle(Origin, TargetSize);

        private Size ImgSize { get; set; }

        private Size TargetSize { get; set; }

        private Point Origin { get; set; }

        /// <summary>
        /// Resize the image.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>Resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;

                graphics.DrawImage(image, 0, 0, width, height);
            }

            return destImage;
        }

        /// <summary>
        /// Get the height of the scaled image according to its given width.
        /// </summary>
        /// <param name="size">The source image size.</param>
        /// <param name="width">The desired image width.</param>
        /// <returns>Size result of the scaling calculation.</returns>
        private Size ScaleByWidth(Size size, float width)
        {
            var ratio = (float)size.Width / (float)size.Height;
            return new Size((int)width, (int)(width / ratio));
        }

        /// <summary>
        /// Get the width of the scaled image according to its given height.
        /// </summary>
        /// <param name="size">The source image size.</param>
        /// <param name="height">The desired image height.</param>
        /// <returns>Size result of the scaling calculation.</returns>
        private Size ScaleByHeight(Size size, float height)
        {
            var ratio = (float)size.Width / (float)size.Height;
            return new Size((int)(height * ratio), (int)height);
        }

        /// <summary>
        /// Get new sizes of the image to resize,
        /// The scale calculation will fit the new size till both width and height are contianed,
        /// So the final image is not cropped and completely fits in the new size.
        /// </summary>
        /// <param name="imgSize">Image size.</param>
        /// <param name="targetSize">Target size.</param>
        /// <returns>Size result of the scaling calculation.</returns>
        private Size AutoScale(Size imgSize, Size targetSize)
        {
            var orgRatio = (float)imgSize.Width / (float)imgSize.Height;

            var newWidth = (float)targetSize.Width == 0 ? (float)targetSize.Height * orgRatio : (float)targetSize.Width;

            var newHeight = (float)targetSize.Height == 0 ? (float)targetSize.Height / orgRatio : (float)targetSize.Height;

            if (orgRatio > 1)
            {
                newHeight = targetSize.Width / orgRatio;
            }
            else
            {
                newWidth = targetSize.Height * orgRatio;
            }

            return new Size((int)newWidth, (int)newHeight);
        }
    }
}
