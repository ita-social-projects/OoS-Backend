namespace OutOfSchool.WebApi.Config.DataAccess
{
    /// <summary>
    /// Contains configuration path keys for Google Cloud Storage.
    /// </summary>
    public static class GcpStorageConfigConstants
    {
        /// <summary>
        /// Points Google Cloud Storage Section.
        /// </summary>
        public const string GcpStorageBaseSection = "GoogleCloudPlatform:Storage:";

        /// <summary>
        /// Points section for images.
        /// </summary>
        public const string GcpStorageImagesConfig = GcpStorageBaseSection + "OosImages";
    }
}