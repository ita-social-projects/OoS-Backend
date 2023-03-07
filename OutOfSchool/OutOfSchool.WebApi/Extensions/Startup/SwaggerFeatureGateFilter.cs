using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OutOfSchool.WebApi.Extensions.Startup;

/// <summary>Checks the current Feature Management configuration and removes all paths with disabled features</summary>
public class SwaggerFeatureGateFilter : IDocumentFilter {

    private readonly IFeatureManager _featureManager;

    public SwaggerFeatureGateFilter(IFeatureManager featureManager) {
        _featureManager = featureManager;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {

        foreach (var apiDescription in context.ApiDescriptions) {
            var actionDescriptor = (ControllerActionDescriptor)apiDescription.ActionDescriptor;

            var attrForController = actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(FeatureGateAttribute), true).Select(a => (FeatureGateAttribute)a).ToList();
            var attrForEndpoint = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(FeatureGateAttribute), true).Select(a => (FeatureGateAttribute)a).ToList();
            if (!attrForController.Any() && !attrForEndpoint.Any()) {
                continue;
            }

            var allControllerAttributesEnabled = attrForController.Select(attr =>
                attr.RequirementType == RequirementType.All
                    ? attr.Features.All<string>(feature => _featureManager.IsEnabledAsync(feature).Result)
                    : attr.Features.Any<string>(feature => _featureManager.IsEnabledAsync(feature).Result))
                .All(flag => flag);

            var allEndpointAttributesEnabled = attrForEndpoint.Select(attr =>
                    attr.RequirementType == RequirementType.All
                        ? attr.Features.All<string>(feature => _featureManager.IsEnabledAsync(feature).Result)
                        : attr.Features.Any<string>(feature => _featureManager.IsEnabledAsync(feature).Result))
                .All(flag => flag);

            if (allControllerAttributesEnabled && allEndpointAttributesEnabled) {
                continue;
            }

            var key = "/" + apiDescription.RelativePath!.TrimEnd('/');

            var operation = (OperationType)Enum.Parse(typeof(OperationType), apiDescription.HttpMethod!, true);

            swaggerDoc.Paths[key].Operations.Remove(operation);

            // drop the entire route if there are no operations left
            if (!swaggerDoc.Paths[key].Operations.Any()) {
                swaggerDoc.Paths.Remove(key);
            }
        }
    }
}