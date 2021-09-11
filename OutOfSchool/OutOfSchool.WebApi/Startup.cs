using System.Globalization;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Extensions.Startup;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Middlewares;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            var proxyOptions = Configuration.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
            app.UseProxy(proxyOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("uk"),
            };

            var requestLocalization = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("uk"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
            };

            app.UseRequestLocalization(requestLocalization);

            app.UseCors("AllowAll");

            app.UseMiddleware<ExceptionMiddlewareExtension>();

            app.UseSwaggerWithVersioning(provider, proxyOptions);

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            // Enable extracting token from QueryString for Hub-connection authorization
            app.UseMiddleware<AuthorizationTokenMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chathub");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppDefaultsConfig>(Configuration.GetSection(AppDefaultsConfig.Name));
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.ApiName = "outofschoolapi";
                    options.Authority = Configuration["Identity:Authority"];

                    options.RequireHttpsMetadata = false;
                });

            services.AddCors(confg =>
                confg.AddPolicy(
                    "AllowAll",
                    p => p.WithOrigins(Configuration["AllowedCorsOrigins"].Split(','))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()));

            services.AddControllers().AddNewtonsoftJson();

            // TODO: Ask frontend if all enums as strings are fine by adding serializer project wide
            // .AddJsonOptions(options =>
            //     options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddDbContext<OutOfSchoolDbContext>(builder =>
                builder.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")))
                .AddCustomDataProtection("WebApi");

            // Add Elasticsearch client
            var elasticConfig = Configuration
                .GetSection(ElasticConfig.Name)
                .Get<ElasticConfig>();
            services.AddElasticsearch(elasticConfig);
            services.AddTransient<IElasticsearchProvider<WorkshopES, WorkshopFilterES>, ESWorkshopProvider>();
            services.AddTransient<IElasticsearchService<WorkshopES, WorkshopFilterES>, ESWorkshopService>();

            // entities services
            services.AddTransient<IAddressService, AddressService>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IChatMessageService, ChatMessageService>();
            services.AddTransient<IChatRoomService, ChatRoomService>();
            services.AddTransient<IChildService, ChildService>();
            services.AddTransient<ICityService, CityService>();
            services.AddTransient<IClassService, ClassService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IDirectionService, DirectionService>();
            services.AddTransient<IFavoriteService, FavoriteService>();
            services.AddTransient<IParentService, ParentService>();
            services.AddTransient<IProviderService, ProviderService>();
            services.AddTransient<IRatingService, RatingService>();
            services.AddTransient<ISocialGroupService, SocialGroupService>();
            services.AddTransient<IStatisticService, StatisticService>();
            services.AddTransient<ITeacherService, TeacherService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IWorkshopService, WorkshopService>();
            services.AddTransient<IWorkshopServicesCombiner, WorkshopServicesCombiner>();

            // entities repositories
            services.AddTransient<IEntityRepository<Address>, EntityRepository<Address>>();
            services.AddTransient<IEntityRepository<Application>, EntityRepository<Application>>();
            services.AddTransient<IEntityRepository<ChatMessage>, EntityRepository<ChatMessage>>();
            services.AddTransient<IEntityRepository<ChatRoom>, EntityRepository<ChatRoom>>();
            services.AddTransient<IEntityRepository<ChatRoomUser>, EntityRepository<ChatRoomUser>>();
            services.AddTransient<IEntityRepository<Child>, EntityRepository<Child>>();
            services.AddTransient<IEntityRepository<City>, EntityRepository<City>>();
            services.AddTransient<IEntityRepository<Favorite>, EntityRepository<Favorite>>();
            services.AddTransient<IEntityRepository<Direction>, EntityRepository<Direction>>();
            services.AddTransient<IEntityRepository<SocialGroup>, EntityRepository<SocialGroup>>();
            services.AddTransient<IEntityRepository<Teacher>, EntityRepository<Teacher>>();
            services.AddTransient<IEntityRepository<User>, EntityRepository<User>>();

            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IClassRepository, ClassRepository>();
            services.AddTransient<IDepartmentRepository, DepartmentRepository>();
            services.AddTransient<IParentRepository, ParentRepository>();
            services.AddTransient<IProviderRepository, ProviderRepository>();
            services.AddTransient<IRatingRepository, RatingRepository>();
            services.AddTransient<IWorkshopRepository, WorkshopRepository>();

            services.AddSingleton(Log.Logger);
            services.AddVersioning();
            var swaggerConfig = Configuration.GetSection(SwaggerConfig.Name).Get<SwaggerConfig>();

            // Required to inject it in OutOfSchool.WebApi.Extensions.Startup.CustomSwaggerOptions class
            services.AddSingleton(swaggerConfig);
            services.AddSwagger(swaggerConfig);

            services.AddProxy();

            services.AddAutoMapper(typeof(MappingProfile));

            services.AddSignalR();
        }
    }
}