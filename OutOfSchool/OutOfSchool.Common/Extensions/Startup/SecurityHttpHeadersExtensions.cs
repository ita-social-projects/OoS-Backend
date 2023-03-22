using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Common.Extensions.Startup;
public static class SecurityHttpHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHttpHeaders(this IApplicationBuilder app, bool isDevelopment)
    {
        var policies = new HeaderPolicyCollection()
                .AddFrameOptionsDeny()
                .AddXssProtectionBlock()
                .AddContentTypeOptionsNoSniff()
                .AddReferrerPolicyStrictOriginWhenCrossOrigin()
                .AddCrossOriginOpenerPolicyReportOnly(builder => builder.SameOrigin())
                .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
                .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
                .RemoveServerHeader()
                .AddPermissionsPolicy(builder =>
                {
                    builder.AddAccelerometer().None();
                    builder.AddAutoplay().None();
                    builder.AddCamera().None();
                    builder.AddEncryptedMedia().None();
                    builder.AddFullscreen().All();
                    builder.AddGeolocation().None();
                    builder.AddGyroscope().None();
                    builder.AddMagnetometer().None();
                    builder.AddMicrophone().None();
                    builder.AddMidi().None();
                    builder.AddPayment().None();
                    builder.AddPictureInPicture().None();
                    builder.AddSyncXHR().None();
                    builder.AddUsb().None();
                });

        AddCspHstsDefinitions(isDevelopment, policies);

        policies.ApplyDocumentHeadersToAllResponses();

        return app.UseSecurityHeaders(policies);
    }

    private static void AddCspHstsDefinitions(bool isDevelopment, HeaderPolicyCollection policy)
    {
        if (!isDevelopment)
        {
            policy.AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().None();
                builder.AddImgSrc().None();
                builder.AddFormAction().Self();
                builder.AddFontSrc().None();
                builder.AddStyleSrc().None();
                builder.AddScriptSrc().None();
                builder.AddBaseUri().Self();
                builder.AddFrameAncestors().None();
                builder.AddCustomDirective("require-trusted-types-for", "'script'");
            });

            policy.AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365);
        }
        else
        {
            // allow swagger UI for dev
            policy.AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().None();
                builder.AddImgSrc().Self().From("data:");
                builder.AddFontSrc().Self().From("fonts.gstatic.com");
                builder.AddStyleSrc().Self().UnsafeInline().From("fonts.googleapis.com");
                builder.AddScriptSrc().Self().UnsafeInline();
                builder.AddBaseUri().Self();
                builder.AddFrameAncestors().None();
            });
        }
    }
}
