using System.Drawing;
using System.IO;

namespace OutOfSchool.WebApi.Util.Pictures
{
    public static class PictureInfoManager
    {
        public static Resolution GetImageResolution(this Stream stream)
        {
            using var image = Image.FromStream(stream);
            return new Resolution{Width = image.Width, Height = image.Height};
        }
    }
}
