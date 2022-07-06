using System;

namespace OutOfSchool.Services.Models.ChatWorkshop;

public class WorkshopInfoForChatList
{
    public Guid Id { get; set; }

    public string ProviderTitle { get; set; }

    public string Title { get; set; }

    public Guid ProviderId { get; set; }
}