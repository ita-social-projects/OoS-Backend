using System.Collections.Generic;

namespace OutOfSchool.WebApi.Config
{
    public class SwaggerConfig
    {
        public const string Name = "Swagger";

        public IdentityAccessConfig IdentityAccess { get; set; }

        public ApiInfoConfig ApiInfo { get; set; }

        public SecurityDefinitionsConfig SecurityDefinitions { get; set; }
    }

    public class IdentityAccessConfig
    {
        public const string Name = "IdentityAccess";

        public string BaseUrl { get; set; }
    }

    public class ApiInfoConfig
    {
        public const string Name = "ApiInfo";

        public string Title { get; set; }

        public string Description { get; set; }

        public ContactConfig Contact { get; set; }

        public string DeprecationMessage { get; set; }
    }

    public class ContactConfig
    {
        public const string Name = "Contact";

        public string FullName { get; set; }

        public string Email { get; set; }
    }

    public class SecurityDefinitionsConfig
    {
        public const string Name = "SecurityDefinitions";

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> AccessScopes { get; set; }
    }
}