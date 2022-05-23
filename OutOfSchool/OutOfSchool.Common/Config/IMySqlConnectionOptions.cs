namespace OutOfSchool.Common.Config
{
    public interface IMySqlConnectionOptions
    {
        public bool UseOverride { get; set; }

        public string Server { get; set; }

        public uint Port { get; set; }

        public string Database { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public bool IsGuidFormatBinary16 { get; set; }
    }
}