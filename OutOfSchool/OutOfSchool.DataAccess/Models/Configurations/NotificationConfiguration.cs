using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.Configurations;

internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId);

        builder
            .Property(b => b.Data)
            .HasConversion(
                v => JsonSerializerHelper.Serialize(v, null),
                v => JsonSerializerHelper.Deserialize<Dictionary<string, string>>(v, null));
    }
}