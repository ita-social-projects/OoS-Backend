using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;

namespace OutOfSchool.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("outofschoolapi.read"),
                new ApiScope("outofschoolapi.write")
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("outofschoolapi")
            {
                Scopes = new List<string> { "outofschoolapi.read", "outofschoolapi.write"},
                ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                UserClaims = new List<string> {"role"}
            }
        };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client  Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = {new Secret("SuperSecretPassword".Sha256())},

                    AllowedScopes = { "outofschoolapi.read", "outofschoolapi.write" }
                },

                new Client
                {
                    ClientId = "angular",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris = { "http://localhost:4200" },
                    PostLogoutRedirectUris = { "http://localhost:4200" },
                    AllowedCorsOrigins = { "http://localhost:4200" },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "outofschoolapi.read", "outofschoolapi.write"
                    },

                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                }
            };
    }
}