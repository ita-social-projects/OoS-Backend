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
    public static IApplicationBuilder UseSecurityHttpHeaders(this IApplicationBuilder app)
    {
        var policiesHeaders = new HeaderPolicyCollection()
                .AddDefaultSecurityHeaders()
                .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 31449600)
                .AddReferrerPolicyStrictOrigin()
                .AddPermissionsPolicy(builder =>
                {
                    builder.AddMicrophone();
                    builder.AddGeolocation()
                        .Self();
                    builder.AddCamera();
                })
                .AddContentSecurityPolicyReportOnly(builder =>
                {
                    builder.AddDefaultSrc()
                        .Self();
                    builder.AddObjectSrc()
                        .None();
                    builder.AddStyleSrc()
                        .Self()
                        .UnsafeInline()
                        .From("fonts.googleapis.com");
                    builder.AddFontSrc()
                        .From("fonts.gstatic.com");
                    builder.AddScriptSrc()
                        .Self();
                    builder.AddBaseUri()
                        .Self();
                    builder.AddImgSrc()
                        .From("https://*")
                        .Self()
                        .Data();
                    builder.AddCustomDirective("script-src-elem", "self");
                    builder.AddCustomDirective("trusted-types", "angular");
                    builder.AddCustomDirective("require-trusted-types-for", "script");
                });

        return app.UseSecurityHeaders(policiesHeaders);
    }
}
