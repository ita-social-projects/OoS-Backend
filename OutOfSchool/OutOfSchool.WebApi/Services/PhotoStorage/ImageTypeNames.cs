namespace OutOfSchool.WebApi.Services.PhotoStorage
{
    // Summary:
    // Specifies different content types for image.
    public static class ImageTypeNames
    {
        // Summary:
        // Raster-graphics image format that supports lossless data compression.
        public const string Png = "image/png";

        // Summary:
        // Digital image format which contains compressed image data.
        public const string Jpeg = "image/jpeg";

        // Summary:
        // Image format that provides superior lossless and lossy compression for images on the web.
        public const string Webp = "image/webp";
    }
}
