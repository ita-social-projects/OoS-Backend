using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ParentRepository : EntityRepository<Parent>, IParentRepository
    {
        private readonly OutOfSchoolDbContext db;

        public ParentRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="entity">Entity to create.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public new async Task<Parent> Create(Parent entity)
        {
            await db.Parents.AddAsync(entity);
            await db.SaveChangesAsync();

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == entity.UserId).ConfigureAwait(false);
            user.IsRegistered = true;
            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return await Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public IEnumerable<Tuple<long, string, string>> GetUsersByParents(IEnumerable<long> parents)
        {
            var parentWithUser = db.Parents.AsEnumerable().Join(parents, o => o.Id, i => i, (o, i) => new { o.Id, o.UserId });
            var usersInfo = db.Users.AsEnumerable().Join(parentWithUser, o => o.Id, i => i.UserId, (o, i) => new { ParentId = i.Id, o.FirstName, o.LastName });

            List<Tuple<long, string, string>> usersFinalInfo = new List<Tuple<long, string, string>>();

            foreach (var user in usersInfo)
            {
                usersFinalInfo.Add(new Tuple<long, string, string>(user.ParentId, user.FirstName, user.LastName));
            }

            return usersFinalInfo;
        }
    }
}
