using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository;

public class ChatMessageRepository : SensitiveEntityRepository<ChatMessageWorkshop>, IChatMessageRepository
{
    public ChatMessageRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<int> CountUnreadMessages(Guid workshopId)
    {
        return dbContext.ChatMessageWorkshops
            .Include(chr => chr.ChatRoom)
            .Where(x => x.ChatRoom.WorkshopId == workshopId && x.ReadDateTime == null && !x.SenderRoleIsProvider)
            .CountAsync();
    }
}