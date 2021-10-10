using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(x => x.Id)
                .IsClustered(false);

            builder.Property(x => x.Text)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.CreatedTime)
                .IsRequired();

            builder.Property(x => x.IsRead)
                .IsRequired();

            builder.HasOne(m => m.ChatRoom)
                .WithMany(r => r.ChatMessages)
                .HasForeignKey(r => r.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
