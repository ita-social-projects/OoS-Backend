using System;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public static class AddCustomKeyManagementExtension
    {
        public static IIdentityServerBuilder AddCustomKeyManagement<TContext>(
            this IIdentityServerBuilder builder,
            Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            builder.Services
                .AddDbContext<TContext>(optionsAction)
                .AddLazyCache()
                .AddSingleton<IKeyManager, KeyManager>()
                .AddTransient<ISigningCredentialStore, SigningCredentialsStore>()
                .AddTransient<IValidationKeysStore, ValidationKeyStore>();
            return builder;
        }
    }
}