using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.Services.Extensions
{
    public static class DataProtectionExtensions
    {
        public static IDataProtectionBuilder AddCustomDataProtection(this IServiceCollection services, string applicationName)
        {
            return services.AddDataProtection().SetApplicationName(applicationName).PersistKeysToDbContext<OutOfSchoolDbContext>()
                ;
        }
    }
}