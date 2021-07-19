namespace OutOfSchool.IdentityServer.Config
{
    public class IdentityAccessOptions
    {
        public readonly string Name = "IdentityAccessConfig";

        public AdditionalIdentityClients[] AdditionalIdentityClients { get; set; }
    }
}