using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

public class WorkshopInfoForChatList
{
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }

    public string ProviderTitle { get; set; }

    public string Title { get; set; }

    public Guid ProviderId { get; set; }
}