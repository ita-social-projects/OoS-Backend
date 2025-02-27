using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

public class ChatRoomWorkshopForChatList
{
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }

    public Guid WorkshopId { get; set; }

    public Guid ParentId { get; set; }

    public ParentInfoForChatList Parent { get; set; }

    public WorkshopInfoForChatList Workshop { get; set; }

    public int NotReadByCurrentUserMessagesCount { get; set; }

    public ChatMessageInfoForChatList LastMessage { get; set; }

    public bool IsBlockedByProvider { get; set; }
}