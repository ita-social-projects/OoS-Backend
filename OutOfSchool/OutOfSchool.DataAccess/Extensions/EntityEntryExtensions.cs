using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OutOfSchool.Services.Extensions
{
    public static class EntityEntryExtensions
    {
        public static string GetColumnName(this PropertyEntry entry) =>
            entry.Metadata.GetColumnName(StoreObjectIdentifier.Create(entry.EntityEntry.Metadata, StoreObjectType.Table).Value);
    }
}
