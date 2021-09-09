using System;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatMessageWorkshopDto
    {
        // TODO: consider to make GUID
        public long Id { get; set; }

        // TODO: consider to make GUID
        public long ChatRoomId { get; set; }

        public string Text { get; set; }

        public bool SenderRoleIsProvider { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }

        public DateTimeOffset? ReadDateTime { get; set; }
    }
}
