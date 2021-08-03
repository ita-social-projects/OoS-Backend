using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Middlewares;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
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
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Out Of School API");
                c.OAuthClientId("Swagger");
                c.OAuthUsePkce();
            });

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

            services.AddControllers();

            services.AddDbContext<OutOfSchoolDbContext>(builder =>
                builder.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Add Elasticsearch client
            services.AddElasticsearch(Configuration);
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

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Set grouping for elements.
                c.TagActionsBy(api =>
                {
                    // If there is a groupName that is specified, then use it.
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName };
                    }

                    // else use the controller name which is the default used by Swashbuckle.
                    if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        return new[] { controllerActionDescriptor.ControllerName };
                    }

                    throw new InvalidOperationException("Unable to determine tag for endpoint.");
                });
                c.OperationFilter<AuthorizeCheckOperationFilter>();
                var baseUrl = Configuration["SwaggerIdentityAccess:BaseUrl"];
                c.AddSecurityDefinition("Identity server", new OpenApiSecurityScheme
                {
                    Description = "Identity server",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{baseUrl}/connect/authorize", UriKind.Absolute),
                            TokenUrl = new Uri($"{baseUrl}/connect/token", UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid outofschoolapi.read offline_access", "Scopes" },
                            },
                        },
                    },
                });
                c.DocInclusionPredicate((name, api) => true);
            });

            services.AddAutoMapper(typeof(Startup));

            services.AddSignalR();
        }
    }
}