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
    private readonly OutOfSchoolDbContext db;

    public ChatMessageRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public async Task<int> CountUnreadMessagesAsync(Guid workshopId)
    {
        return await db.ChatMessageWorkshops
            .Include(chr => chr.ChatRoom)
            .Where(x => x.ChatRoom.WorkshopId == workshopId && x.ReadDateTime == null && !x.SenderRoleIsProvider)
            .CountAsync();
    }
}