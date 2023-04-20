using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Models.Configurations;

internal class ChatMessageWorkshopConfiguration : IEntityTypeConfiguration<ChatMessageWorkshop>
{
    public void Configure(EntityTypeBuilder<ChatMessageWorkshop> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(m => m.ChatRoom)
            .WithMany(r => r.ChatMessages)
            .HasForeignKey(r => r.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}