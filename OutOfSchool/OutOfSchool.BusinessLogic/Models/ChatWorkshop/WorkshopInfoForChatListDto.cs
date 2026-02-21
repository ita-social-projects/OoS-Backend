using OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class WorkshopInfoForChatListDto
{
    public Guid Id { get; set; }

    public string ProviderTitle { get; set; }

    public string Title { get; set; }

    public Guid ProviderId { get; set; }
}

public static class WorkshopInfoForChatListDtoExtensions
{
    public static WorkshopInfoForChatListDto ToChatListDto(this Workshop model)
        => new()
        {
            Id = model.Id,
            ProviderTitle = model.Provider?.FullTitle,
            Title = model.Title,
            ProviderId = model.ProviderId,
        };

    public static WorkshopInfoForChatListDto ToChatListDto(this WorkshopInfoForChatList model)
        => new()
        {
            Id = model.Id,
            ProviderTitle = model.ProviderTitle,
            Title = model.Title,
            ProviderId = model.ProviderId,
        };
}