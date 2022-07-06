using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Models.Configurations;

internal class ChatRoomWorkshopConfiguration : IEntityTypeConfiguration<ChatRoomWorkshop>
{
    public void Configure(EntityTypeBuilder<ChatRoomWorkshop> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(r => r.Parent)
            .WithMany(p => p.ChatRooms)
            .HasForeignKey(r => r.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(r => r.Workshop)
            .WithMany(w => w.ChatRooms)
            .HasForeignKey(r => r.WorkshopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}