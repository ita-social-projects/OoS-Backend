using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatRoomWorkshop
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        [Required]
        public long ParentId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual Parent Parent { get; set; }

        public virtual ICollection<ChatMessageWorkshop> ChatMessages { get; set; }
    }
}
