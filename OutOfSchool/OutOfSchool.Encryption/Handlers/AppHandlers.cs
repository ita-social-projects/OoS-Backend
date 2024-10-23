using System.Diagnostics.CodeAnalysis;
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
                "api/{version:apiVersion}/certificate",
                ([FromServices] EUSignOAuth2Service euSignOAuth2Service) =>
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
            .MapToApiVersion(AppConstants.API_V1);

        app.MapPost(
                "api/{version:apiVersion}/decrypt", (
                    [NotNull] EnvelopedUserInfoResponse encryptedUserInfo,
                    [FromServices] EUSignOAuth2Service euSignOAuth2Service) =>
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
            .MapToApiVersion(AppConstants.API_V1);

        return app;
    }
}