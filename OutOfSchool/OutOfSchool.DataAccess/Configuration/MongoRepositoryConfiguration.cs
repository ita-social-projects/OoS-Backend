namespace OutOfSchool.Services.Configuration
{
    public class MongoRepositoryConfiguration
    {
        public const string SectionName = nameof(MongoRepositoryConfiguration);

        public string ConnectionString { get; set; } = "mongodb://localhost:27017";

        public string PicturesTableName { get; set; } = "Pictures";
    }
}
