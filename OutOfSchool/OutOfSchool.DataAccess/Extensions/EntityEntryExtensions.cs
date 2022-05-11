using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OutOfSchool.Services.Extensions
{
    public static class EntityEntryExtensions
    {
        public static string GetTableName(this EntityEntry entry) =>
            entry.Metadata.GetAnnotation("Relational:TableName").Value.ToString();

        public static string GetColumnName(this PropertyEntry entry) =>
            entry.Metadata.GetColumnName(StoreObjectIdentifier.Create(entry.EntityEntry.Metadata, StoreObjectType.Table).Value);
    }
}
