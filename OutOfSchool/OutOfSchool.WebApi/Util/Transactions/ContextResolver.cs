using System;
using AutoMapper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public class ContextResolver
    {
        private readonly IServiceProvider serviceProvider;

        public ContextResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public DbContext GetRequiredContext(DbContextName dbContextName)
        {
            var type = DbContextTypes.GetTypeByName(dbContextName);
            return (DbContext)serviceProvider.GetRequiredService(type);
        }
    }
}
