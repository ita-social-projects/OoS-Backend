using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Extensions
{
    public static class EntityEntryExtensions
    {
        public static string GetColumnName(this PropertyEntry entry) =>
            entry.Metadata.GetColumnName(StoreObjectIdentifier.Create(entry.EntityEntry.Metadata, StoreObjectType.Table).Value);

        public static int? GetPropertyMaxLength<T>(this DbContext dbContext, string propertyName)
            where T : class, IKeyedEntity
            => dbContext.Model.FindEntityType(typeof(T))?.GetProperty(propertyName)?.GetMaxLength();
    }
}
