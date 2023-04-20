using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OutOfSchool.WebApi.Config;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OutOfSchool.WebApi.Extensions.Startup;

/// <summary>
///     Configures the Swagger generation options.
/// </summary>
/// <remarks>
///     This allows API versioning to define a Swagger document per API version after the
///     <see cref="IApiVersionDescriptionProvider" /> service has been resolved from the service container.
/// </remarks>
public class CustomSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;
    private readonly SwaggerConfig swaggerConfig;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomSwaggerOptions" /> class.
    /// </summary>
    /// <param name="provider">
    ///     The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger
    ///     documents.
    /// </param>
    /// <param name="swaggerConfig">
    ///     App settings from <see cref="swaggerConfig"/> used to populate <see cref="OpenApiInfo"/>
    ///     documentation.
    /// </param>
    public CustomSwaggerOptions(IApiVersionDescriptionProvider provider, SwaggerConfig swaggerConfig)
    {
        this.provider = provider;
        this.swaggerConfig = swaggerConfig;
    }

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        // add a swagger document for each discovered API version
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, ApiInfo(description, swaggerConfig.ApiInfo));
        }
    }

    public void Configure(string name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private static OpenApiInfo ApiInfo(ApiVersionDescription description, ApiInfoConfig config)
    {
        OpenApiInfo info = new OpenApiInfo
        {
            Title = config.Title,
            Version = description.ApiVersion.ToString(),
            Description = config.Description,
            Contact = new OpenApiContact {Name = config.Contact.FullName, Email = config.Contact.Email},
        };

        if (description.IsDeprecated)
        {
            info.Description += $" {config.DeprecationMessage}";
        }

        return info;
    }
}