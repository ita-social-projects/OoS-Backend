using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OutOfSchool.WebApi.Extensions.Startup
{
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomSwaggerOptions" /> class.
        /// </summary>
        /// <param name="provider">
        ///     The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger
        ///     documents.
        /// </param>
        public CustomSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, ApiInfo(description));
            }
        }

        public void Configure(string name, SwaggerGenOptions options)
        {
            Configure(options);
        }

        private static OpenApiInfo ApiInfo(ApiVersionDescription description)
        {
            OpenApiInfo info = new OpenApiInfo
            {
                Title = "Out of School API",
                Version = description.ApiVersion.ToString(),
                Description = "Out of School API",
                Contact = new OpenApiContact {Name = "Admin", Email = "PozashkilliaUA@gmail.com"},
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}