using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository;

public interface IChatMessageRepository : ISensitiveEntityRepository<ChatMessageWorkshop>
{
    Task<int> CountUnreadMessages(Guid workshopId);
}