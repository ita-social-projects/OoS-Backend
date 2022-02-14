using System;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatMessageWorkshopDto
    {
        public Guid Id { get; set; }

        public Guid ChatRoomId { get; set; }

        public string Text { get; set; }

        public bool SenderRoleIsProvider { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }

        public DateTimeOffset? ReadDateTime { get; set; }
    }
}
