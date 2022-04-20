namespace OutOfSchool.WebApi.Config
{
    public class GRPCConfig
    {
        public const string Name = "GRPC";

        public bool Enabled { get; set; }

        public string Host { get; set; }
    }
}
