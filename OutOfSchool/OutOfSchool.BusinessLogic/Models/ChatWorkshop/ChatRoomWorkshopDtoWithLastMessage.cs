﻿namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class ChatRoomWorkshopDtoWithLastMessage : ChatRoomWorkshopDto
{
    public int NotReadByCurrentUserMessagesCount { get; set; }

    public ChatMessageWorkshopDto LastMessage { get; set; }
}