﻿namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class WorkshopInfoForChatListDto
{
    public Guid Id { get; set; }

    public string ProviderTitle { get; set; }

    public string Title { get; set; }

    public Guid ProviderId { get; set; }
}