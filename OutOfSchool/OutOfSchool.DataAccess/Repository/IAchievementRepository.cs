﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IAchievementRepository : IEntityRepositorySoftDeleted<Guid, Achievement>
{
    Task<IEnumerable<Achievement>> GetByWorkshopId(Guid workshopId);

    Task<Achievement> Create(Achievement achievement, List<Guid> childrenIDs, List<string> teachers);

    Task<Achievement> Update(Achievement achievement, List<Guid> childrenIDs, List<string> teachers);
}