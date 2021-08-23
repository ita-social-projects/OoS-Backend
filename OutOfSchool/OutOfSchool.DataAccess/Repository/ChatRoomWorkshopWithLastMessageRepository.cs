using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository
{
    public class ChatRoomWorkshopWithLastMessageRepository
    {
        private readonly OutOfSchoolDbContext dbContext;
        private readonly DbSet<ChatRoomWorkshop> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomWorkshopWithLastMessageRepository"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        public ChatRoomWorkshopWithLastMessageRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = this.dbContext.Set<ChatRoomWorkshop>();
        }
    }
}
