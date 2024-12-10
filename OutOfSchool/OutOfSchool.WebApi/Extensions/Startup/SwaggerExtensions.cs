using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace OutOfSchool.WebApi.Extensions.Startup;

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

    public static IServiceCollection AddSwagger(this IServiceCollection services, SwaggerConfig config)
    {
        var identityBaseUrl = config.IdentityAccess.BaseUrl;

        services
            .ConfigureOptions<CustomSwaggerOptions>()
            .AddSwaggerGen(c =>
            {
                // Set the comments path for the Swagger JSON and UI.
                c.IncludeXmlComments(XmlCommentsFilePath);

                c.DocumentFilter<SwaggerFeatureGateFilter>();
                c.OperationFilter<AuthorizeCheckOperationFilter>();
                c.AddSecurityDefinition(config.SecurityDefinitions.Title, new OpenApiSecurityScheme
                {
                    Description = config.SecurityDefinitions.Description,
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{identityBaseUrl}/connect/authorize?prompt=login",
                                UriKind.Absolute),
                            TokenUrl = new Uri($"{identityBaseUrl}/connect/token", UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                            {
                                {string.Join(" ", config.SecurityDefinitions.AccessScopes), "Scopes"},
                            },
                        },
                    },
                });
                c.UseOneOfForPolymorphism();
                c.UseAllOfForInheritance();
            });

        return services;
    }

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