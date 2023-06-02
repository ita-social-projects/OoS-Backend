using OutOfSchool.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Repository;

public interface IInstitutionHierarchyRepository : IEntityRepositoryBase<Guid, InstitutionHierarchy>
{
    Task<InstitutionHierarchy> Update(InstitutionHierarchy entity, List<long> directionsIds);
}