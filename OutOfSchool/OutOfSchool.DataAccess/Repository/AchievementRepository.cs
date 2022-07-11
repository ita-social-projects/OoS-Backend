using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

/// <summary>
/// Repository for accessing the Achievement table in database.
/// </summary>
public class AchievementRepository : EntityRepositoryBase<Guid, Achievement>, IAchievementRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementRepository"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public AchievementRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// Get elements by Workshop Id.
    /// </summary>
    /// <param name="workshopId">GUID Workshop Id.</param>
    /// /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<IEnumerable<Achievement>> GetByWorkshopId(Guid workshopId)
    {
        var achievements = dbSet.Where(a => a.WorkshopId == workshopId);

        return await Task.FromResult(achievements);
    }

    /// <summary>
    /// Add new element.
    /// </summary>
    /// <param name="achievement">Entity to create.</param>
    /// <param name="childrenIDs">GUID List of Children.</param>
    /// <param name="teachers">String List of Teachers.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<Achievement> Create(Achievement achievement, List<Guid> childrenIDs, List<string> teachers)
    {
        achievement.Children = dbContext.Children.Where(w => childrenIDs.Contains(w.Id))
            .ToList();

        achievement.Teachers = new List<AchievementTeacher>();

        foreach (string teacher in teachers)
        {
            achievement.Teachers.Add(new AchievementTeacher { Title = teacher, Achievement = achievement });
        }

        await dbSet.AddAsync(achievement);
        await dbContext.SaveChangesAsync();

        return await Task.FromResult(achievement);
    }

    /// <summary>
    /// Update information about element.
    /// </summary>
    /// <param name="achievement">Entity to update.</param>
    /// <param name="childrenIDs">GUID List of Children.</param>
    /// <param name="teachers">String List of Teachers.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<Achievement> Update(Achievement achievement, List<Guid> childrenIDs, List<string> teachers)
    {
        var newAchievement = await GetById(achievement.Id);

        dbContext.Entry(newAchievement).CurrentValues.SetValues(achievement);

        newAchievement.Children.RemoveAll(x => !childrenIDs.Contains(x.Id));
        var exceptChildrenIDs = childrenIDs.Where(p => newAchievement.Children.All(x => x.Id != p));
        newAchievement.Children.AddRange(dbContext.Children.Where(w => exceptChildrenIDs.Contains(w.Id)).ToList());

        newAchievement.Teachers.RemoveAll(x => !teachers.Contains(x.Title));

        var exceptTeachers = teachers.Where(p => newAchievement.Teachers.All(x => !x.Title.Equals(p)));
        foreach (var teacher in exceptTeachers)
        {
            newAchievement.Teachers.Add(new AchievementTeacher { Title = teacher, Achievement = newAchievement });
        }

        dbContext.Entry(newAchievement).State = EntityState.Modified;

        await this.dbContext.SaveChangesAsync();
        return newAchievement;
    }

}