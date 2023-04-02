using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Config.ExternalUriModels;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Util;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Extensions;

public static class AuthCommonServiceExtensions
{
    public static void AddAuthCommon(this IServiceCollection services, ConfigurationManager config, bool isDevelopment)
    {
        services.Configure<AuthServerConfig>(config.GetSection(AuthServerConfig.Name));

        // ExternalUris options
        services.Configure<AngularClientScopeExternalUrisConfig>(
            config.GetSection(AngularClientScopeExternalUrisConfig.Name));

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // GRPC options
        services.Configure<GRPCConfig>(config.GetSection(GRPCConfig.Name));

        var mailConfig = config
            .GetSection(EmailOptions.SectionName)
            .Get<EmailOptions>();
        services.AddEmailSender(
            isDevelopment,
            mailConfig.SendGridKey,
            builder => builder.Bind(config.GetSection(EmailOptions.SectionName)));

        var commonAssembly = typeof(AuthCommonServiceExtensions).Assembly;

        services.AddControllersWithViews()
            .AddApplicationPart(commonAssembly)
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResource));
            })
            .AddRazorRuntimeCompilation();

        services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
        {
            options.FileProviders.Add(new EmbeddedFileProvider(commonAssembly));
        });
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddTransient<IParentRepository, ParentRepository>();
        services.AddTransient<IEntityRepository<long, PermissionsForRole>, EntityRepository<long, PermissionsForRole>>();
        services.AddTransient<IProviderAdminRepository, ProviderAdminRepository>();
        services.AddTransient<IProviderAdminService, ProviderAdminService>();
        services.AddTransient<IUserManagerAdditionalService, UserManagerAdditionalService>();
        services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
        services.AddTransient<IRegionAdminRepository, RegionAdminRepository>();
        services.AddTransient<ICommonMinistryAdminService<MinistryAdminBaseDto>,
            CommonMinistryAdminService<Guid, InstitutionAdmin, MinistryAdminBaseDto, IInstitutionAdminRepository>>();
        services.AddTransient<ICommonMinistryAdminService<RegionAdminBaseDto>,
            CommonMinistryAdminService<long, RegionAdmin, RegionAdminBaseDto, IRegionAdminRepository>>();

        services.AddTransient<IEntityRepository<long, ProviderAdminChangesLog>, EntityRepository<long, ProviderAdminChangesLog>>();
        services.AddTransient<IProviderAdminChangesLogService, ProviderAdminChangesLogService>();

        // Register the Permission policy handlers
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
        services.AddGrpc();
    }
}