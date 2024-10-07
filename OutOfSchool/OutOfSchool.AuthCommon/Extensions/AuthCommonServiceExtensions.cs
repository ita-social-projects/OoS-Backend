using Microsoft.AspNetCore.Mvc.DataAnnotations;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Config.ExternalUriModels;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Util;
using OutOfSchool.AuthCommon.Validators;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;

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
        services.Configure<GrpcConfig>(config.GetSection(GrpcConfig.Name));

        var mailConfig = config
            .GetSection(EmailOptions.SectionName)
            .Get<EmailOptions>();

        services.AddEmailSender(isDevelopment, mailConfig?.SendGridKey);
        services.AddEmailSenderService(builder => builder.Bind(config.GetSection(EmailOptions.SectionName)));

        services.Configure<ChangesLogConfig>(config.GetSection(ChangesLogConfig.Name));
        services.Configure<HostsConfig>(config.GetSection(HostsConfig.Name));

        services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (_, factory) =>
                    factory.Create(typeof(SharedResource));
            });
        services.AddSingleton<IValidationAttributeAdapterProvider, CustomClientValidationProvider>();
        services.AddRazorPages();
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddTransient(typeof(IEntityAddOnlyRepository<,>), typeof(EntityRepository<,>));
        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
        services.AddTransient(typeof(IEntityRepositorySoftDeleted<,>), typeof(EntityRepositorySoftDeleted<,>));

        services.AddTransient<IParentRepository, ParentRepository>();
        services.AddTransient<IEmployeeRepository, EmployeeRepository>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IUserManagerAdditionalService, UserManagerAdditionalService>();
        services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
        services.AddTransient<IRegionAdminRepository, RegionAdminRepository>();
        services.AddTransient<ICommonMinistryAdminService<MinistryAdminBaseDto>,
            CommonMinistryAdminService<Guid, InstitutionAdmin, MinistryAdminBaseDto, IInstitutionAdminRepository>>();
        services.AddTransient<ICommonMinistryAdminService<RegionAdminBaseDto>,
            CommonMinistryAdminService<long, RegionAdmin, RegionAdminBaseDto, IRegionAdminRepository>>();
        services.AddTransient<IAreaAdminRepository, AreaAdminRepository>();
        services.AddTransient<ICommonMinistryAdminService<AreaAdminBaseDto>,
            CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, IAreaAdminRepository>>();

        services.AddTransient<IEmployeeChangesLogService, EmployeeChangesLogService>();

        // Register the Permission policy handlers
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
        services.AddGrpc();
    }
}