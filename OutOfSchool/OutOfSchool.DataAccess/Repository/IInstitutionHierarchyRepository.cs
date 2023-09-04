using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Repository;

public interface IInstitutionHierarchyRepository : IEntityRepositorySoftDeleted<Guid, InstitutionHierarchy>
{
    Task<InstitutionHierarchy> Update(InstitutionHierarchy entity, List<long> directionsIds);
}