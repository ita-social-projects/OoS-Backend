namespace OutOfSchool.Common.Config
{
    public interface IMySqlGuidConnectionOptions : IMySqlConnectionOptions
    {
        public string GuidFormat { get; set; }
    }
}