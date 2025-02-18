using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class PositionRepository : SensitiveEntityRepositorySoftDeleted<Position>, IPositionRepository
{
    public PositionRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<Position> Create(Position entity)
    {
        if (entity.PositionType == PositionType.Director)
        {
            var directorExists = await DirectorExistsAsync(entity.ProviderId).ConfigureAwait(false);

            if (directorExists)
            {
                throw new InvalidOperationException("Only one director is allowed per provider.");
            }
        }

        return await base.Create(entity);
    }

    public override async Task<IEnumerable<Position>> Create(IEnumerable<Position> entities)
    {
        var positions = entities.ToList();

        if (positions.Count == 0)
        {
            return positions;
        }

        if (positions.Any(p => p.ProviderId == Guid.Empty))
        {
            throw new ArgumentException("All positions must have a valid ProviderId.");
        }

        var providerId = positions.First().ProviderId;

        var hasDirectorToCreate = positions.Any(e => e.PositionType == PositionType.Director);

        if (hasDirectorToCreate)
        {
            var directorExists = await DirectorExistsAsync(providerId).ConfigureAwait(false);

            if (directorExists)
            {
                throw new InvalidOperationException("Only one director is allowed per provider.");
            }
        }

        return await base.Create(positions);
    }

    public override async Task<Position> Update(Position entity)
    {
        if (entity.PositionType == PositionType.Director)
        {
            var isAlreadyDirector = await dbSet.AnyAsync(p =>
                p.Id == entity.Id &&
                p.PositionType == PositionType.Director);

            if (!isAlreadyDirector)
            {
                throw new InvalidOperationException("Cannot update the entity to a director role if it wasn't already a director.");
            }
        }

        return await base.Update(entity);
    }

    public Task<bool> DirectorExistsAsync(Guid providerId) => dbSet.AnyAsync(p =>
        p.ProviderId == providerId &&
        p.PositionType == PositionType.Director);
}