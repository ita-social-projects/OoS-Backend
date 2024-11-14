#nullable enable

using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Encryption.Constants;
using OutOfSchool.Encryption.Models;
using OutOfSchool.Encryption.Services;

namespace OutOfSchool.Encryption.Handlers;

public static class AppHandlers
{
    public static WebApplication MapAppHandlers(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        app.MapGet(
                "api/v{version:apiVersion}/certificate",
                ([FromServices] IEUSignOAuth2Service euSignOAuth2Service) =>
                {
                    var cert = euSignOAuth2Service.GetEnvelopeCertificateBase64();
                    if (cert != null)
                    {
                        return Results.Ok(cert);
                    }

                    return Results.Problem(
                        title: "Certificate not found",
                        detail: "Certificate was not present in the system.",
                        statusCode: 500);
                })
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(AppConstants.ApiVersion1);

        app.MapPost(
                "api/v{version:apiVersion}/decrypt", (
                    EnvelopedUserInfoResponse? encryptedUserInfo,
                    [FromServices] IEUSignOAuth2Service euSignOAuth2Service) =>
                {
                    if (encryptedUserInfo == null)
                    {
                        return Results.BadRequest();
                    }

                    var result = euSignOAuth2Service.DecryptUserInfo(encryptedUserInfo);

                    if (result != null)
                    {
                        return Results.Ok(result);
                    }

                    return Results.Problem(
                        title: "Decryption failed",
                        detail: "The was an error in decryption process.",
                        statusCode: 500);
                })
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(AppConstants.ApiVersion1);

        return app;
    }
}