using System.Collections.Generic;
using System.Linq;
using IdentityServer4;
using IdentityServer4.Models;

namespace OutOfSchool.IdentityServer.Config
{
    public static class StaticConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"},
                },

                // new identity resource - permissions.
                new IdentityResource
                {
                   Name = "permissions",
                   UserClaims = new List<string> {"permissions"},
                },
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("outofschoolapi.read"),
                new ApiScope("outofschoolapi.write"),
            };

        public static IEnumerable<ApiResource> ApiResources(string apiSecret) => new[]
        {
            new ApiResource("outofschoolapi")
            {
                Scopes = new List<string> {"outofschoolapi.read", "outofschoolapi.write"},
                ApiSecrets = new List<Secret> { new Secret(apiSecret.Sha256()) },
                UserClaims = new List<string> {"role", "permissions"},
            },
        };

        public static IEnumerable<Client> Clients(string clientSecret, IEnumerable<AdditionalIdentityClients> additionalClients)
        {
            // m2m client credentials flow client
            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = {new Secret(clientSecret.Sha256()) },
                    AllowedScopes = {"outofschoolapi.read", "outofschoolapi.write"},
                },
            };

            clients.AddRange(additionalClients.Select(c => new Client
            {
                ClientId = c.ClientId,
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                AllowOfflineAccess = true,

                RedirectUris = c.RedirectUris,
                PostLogoutRedirectUris = c.PostLogoutRedirectUris,
                AllowedCorsOrigins = c.AllowedCorsOrigins,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    "outofschoolapi.read", "outofschoolapi.write",
                },

                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
            }));

            return clients;
        }
    }
}