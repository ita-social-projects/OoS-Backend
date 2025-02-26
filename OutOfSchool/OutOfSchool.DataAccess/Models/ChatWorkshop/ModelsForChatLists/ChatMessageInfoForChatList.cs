using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

public class ChatMessageInfoForChatList
{
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }

    public Guid ChatRoomId { get; set; }

    public string Text { get; set; }

    public bool SenderRoleIsProvider { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? ReadDateTime { get; set; }
}