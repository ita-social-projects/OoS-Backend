using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class ChatRoomUserConfiguration : IEntityTypeConfiguration<ChatRoomUser>
    {
        public void Configure(EntityTypeBuilder<ChatRoomUser> builder)
        {
            builder.HasKey(x => x.Id)
                .IsClustered(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.ChatRoomUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }
}
