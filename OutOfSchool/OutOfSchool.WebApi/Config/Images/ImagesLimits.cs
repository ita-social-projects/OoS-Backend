namespace OutOfSchool.WebApi.Config.Images
{
    /// <summary>
    /// Describes image limits for some Entity.
    /// </summary>
    /// <typeparam name="TEntity">This is an entity for which you can operate with images.</typeparam>
    public class ImagesLimits<TEntity>
    {
        /// <summary>
        /// Gets or sets count maximum count of files for entity.
        /// </summary>
        public int MaxCountOfFiles { get; set; }
    }
}
