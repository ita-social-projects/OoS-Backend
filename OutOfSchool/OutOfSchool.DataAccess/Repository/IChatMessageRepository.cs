using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository;

public interface IChatMessageRepository : ISensitiveEntityRepository<ChatMessageWorkshop>
{
    /// <summary>
    /// Get a number of unread ChatMessages with specified WorkshopId.
    /// </summary>
    /// <param name="workshopId">Workshop's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="int"/> that contains the number of unread messages for the specified Workshop.</returns>
    Task<int> CountUnreadMessages(Guid workshopId);
}