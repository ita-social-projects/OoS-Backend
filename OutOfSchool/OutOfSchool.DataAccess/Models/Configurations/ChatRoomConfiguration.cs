using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.HasKey(x => x.Id)
                .IsClustered(false);

            builder.HasMany(x => x.Users)
                .WithMany(x => x.ChatRooms)
                .UsingEntity<ChatRoomUser>(
                    j => j
                        .HasOne(x => x.User)
                        .WithMany(x => x.ChatRoomUsers)
                        .HasForeignKey(x => x.UserId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne(x => x.ChatRoom)
                        .WithMany(x => x.ChatRoomUsers)
                        .HasForeignKey(x => x.ChatRoomId)
                        .OnDelete(DeleteBehavior.Cascade));

            builder.HasOne(x => x.Workshop)
                .WithMany(x => x.ChatRooms)
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
