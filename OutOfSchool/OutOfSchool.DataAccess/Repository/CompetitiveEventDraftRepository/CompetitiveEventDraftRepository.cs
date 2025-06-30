using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository.CompetitiveEventDraftRepository;

/// <summary>
///     Repository class responsible for managing the storage and retrieval of competitive event draft entities.
/// </summary>
public class CompetitiveEventDraftRepository : EntityRepository<Guid, CompetitiveEventDraft>, ICompetitiveEventDraftRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitiveEventDraftRepository"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public CompetitiveEventDraftRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <inheritdoc/>
    /// <exception cref="EntityDeletedConflictException">Thrown if the entity has already been deleted by another user.</exception>
    /// <exception cref="EntityModifiedConflictException">Thrown if the entity has been modified by another user before deletion.</exception>
    public override async Task Delete(CompetitiveEventDraft entity)
    {
        try
        {
            dbContext.Entry(entity).State = EntityState.Deleted;

            await dbContext.SaveChangesAsync()
                .ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await HandleConcurrencyExceptionAsync(ex))
            {
                throw;
            }
        }
    }

    /// <inheritdoc/>
    /// <exception cref="EntityDeletedConflictException">Thrown if the entity has already been deleted by another user.</exception>
    /// <exception cref="EntityModifiedConflictException">Thrown if the entity has been modified by another user before deletion.</exception>
    public override async Task<CompetitiveEventDraft> Update(CompetitiveEventDraft entity)
    {
        try
        {
            dbContext.Entry(entity).State = EntityState.Modified;

            await dbContext.SaveChangesAsync()
                .ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await HandleConcurrencyExceptionAsync(ex))
            {
                throw;
            }
        }

        return entity;
    }

    private async Task<bool> HandleConcurrencyExceptionAsync(DbUpdateConcurrencyException ex)
    {
        var exceptionEntry = ex.Entries.FirstOrDefault();

        if (exceptionEntry == null)
        {
            return false;
        }

        var databasePropertyValues = await exceptionEntry.GetDatabaseValuesAsync()
            .ConfigureAwait(false);

        if (databasePropertyValues == null)
        {
            throw new EntityDeletedConflictException(
                "The entity has already been deleted by another user.",
                ex);
        }

        throw new EntityModifiedConflictException(
            "The entity was modified by another user.",
            ex,
            databasePropertyValues);
    }
}
