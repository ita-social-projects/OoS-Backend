using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Implementation;
using OutOfSchool.WebApi.Services.Interfaces;
using OutOfSchool.WebApi.Services.Mapping;

namespace OutOfSchool.WebApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var childMapper = new MapperConfiguration(x => x.AddProfile(new ChildMapperProfile())).CreateMapper();
            var socialGroupMapper = new MapperConfiguration(x => x.AddProfile(new SocialGroupMapperProfile())).CreateMapper();
            var organizationMapper = new MapperConfiguration(x => x.AddProfile(new OrganizationMapperProfile())).CreateMapper();
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.ApiName = "outofschoolapi";
                    options.Authority = Configuration["Identity:Authority"];

                    options.RequireHttpsMetadata = false;
                });

            services.AddCors(confg =>
                confg.AddPolicy("AllowAll",
                    p => p.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()));

            services.AddControllers();
            
            services.AddDbContext<OutOfSchoolDbContext>(builder =>
                builder.UseSqlServer(Configuration.GetConnectionString("OutOfSchoolConnectionString")));

            services.AddTransient<IChildService, ChildService>();
            services.AddTransient<IEntityRepository<Child>, EntityRepository<Child>>();
            services.AddSingleton(childMapper);
            services.AddSingleton(socialGroupMapper);

            services.AddTransient<IOrganizationService, OrganizationService>();           
            services.AddTransient<IOrganizationRepository, OrganizationRepository>();
            services.AddSingleton(organizationMapper);
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllowAll");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Out Of School API");
            });
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
