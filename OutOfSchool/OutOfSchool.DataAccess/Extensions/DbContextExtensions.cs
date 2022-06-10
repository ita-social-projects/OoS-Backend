using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Extensions
{
    public static class DbContextExtensions
    {
        public static int? GetPropertyMaxLength<T>(this DbContext dbContext, string propertyName)
            where T : class, IKeyedEntity
            => dbContext.Model.FindEntityType(typeof(T))?.GetProperty(propertyName)?.GetMaxLength();
    }
}
