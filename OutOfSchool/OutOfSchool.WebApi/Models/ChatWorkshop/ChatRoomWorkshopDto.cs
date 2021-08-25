using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatRoomWorkshopDto
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        [Required]
        public long ParentId { get; set; }

        public WorkshopInfoForChatListDto Workshop { get; set; }

        public ParentDtoWithContactInfo Parent { get; set; }
    }
}
