using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OutOfSchool.Common.Config;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Extensions.Startup
{
    /// <summary>
    ///     Swagger Extensions.
    /// </summary>
    public static class SwaggerExtensions
    {
        private static string XmlCommentsFilePath
        {
            get
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                return Path.Combine(AppContext.BaseDirectory, xmlFile);
            }
        }


        /// <summary>
        ///     Add Swagger Configuration dependencies.
        /// </summary>
        public static IServiceCollection AddSwagger(this IServiceCollection services, SwaggerConfig config)
        {
            var identityBaseUrl = config.IdentityAccess.BaseUrl;

            services
                .ConfigureOptions<CustomSwaggerOptions>()
                .AddSwaggerGen(c =>
                {
                    // Set the comments path for the Swagger JSON and UI.
                    c.IncludeXmlComments(XmlCommentsFilePath);

                    c.OperationFilter<AuthorizeCheckOperationFilter>();
                    c.AddSecurityDefinition("Identity server", new OpenApiSecurityScheme
                    {
                        Description = "Identity server",
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{identityBaseUrl}/connect/authorize", UriKind.Absolute),
                                TokenUrl = new Uri($"{identityBaseUrl}/connect/token", UriKind.Absolute),
                                Scopes = new Dictionary<string, string>
                                {
                                    {"openid outofschoolapi.read offline_access", "Scopes"},
                                },
                            },
                        },
                    });
                });

            return services;
        }

        /// <summary>
        ///     Add Swagger dependencies.
        /// </summary>
        public static IApplicationBuilder UseSwaggerWithVersioning(
            this IApplicationBuilder app,
            IApiVersionDescriptionProvider provider,
            ReverseProxyOptions options)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    var basePath = options.BasePath;

                    var swaggerEndpoint = string.IsNullOrEmpty(basePath)
                        ? $"/swagger/{description.GroupName}/swagger.json"
                        : $"{basePath}/swagger/{description.GroupName}/swagger.json";

                    c.SwaggerEndpoint(swaggerEndpoint, description.GroupName.ToUpperInvariant());
                }

                c.OAuthClientId("Swagger");
                c.OAuthUsePkce();
            });

            return app;
        }
    }
}