using FluentValidation;
using FluentValidation.AspNetCore;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Config.ExternalUriModels;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Util;
using OutOfSchool.AuthCommon.Validators;
using OutOfSchool.AuthCommon.ViewModels;
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

        services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (_, factory) =>
                    factory.Create(typeof(SharedResource));
            });
        services.AddRazorPages();
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
        services.AddTransient<IParentRepository, ParentRepository>();
        services.AddTransient<IProviderAdminRepository, ProviderAdminRepository>();
        services.AddTransient<IProviderAdminService, ProviderAdminService>();
        services.AddTransient<IUserManagerAdditionalService, UserManagerAdditionalService>();
        services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
        services.AddTransient<IRegionAdminRepository, RegionAdminRepository>();
        services.AddTransient<ICommonMinistryAdminService<MinistryAdminBaseDto>,
            CommonMinistryAdminService<Guid, InstitutionAdmin, MinistryAdminBaseDto, IInstitutionAdminRepository>>();
        services.AddTransient<ICommonMinistryAdminService<RegionAdminBaseDto>,
            CommonMinistryAdminService<long, RegionAdmin, RegionAdminBaseDto, IRegionAdminRepository>>();

        services.AddTransient<IProviderAdminChangesLogService, ProviderAdminChangesLogService>();

        // Register the Permission policy handlers
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
        services.AddGrpc();

        services.AddFluentValidationAutoValidation();
        services.AddScoped<IValidator<RegisterViewModel>, RegisterViewModelValidator>();
    }
}