using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IInstitutionHierarchyRepository : IEntityRepositorySoftDeleted<Guid, InstitutionHierarchy>
{
    Task<InstitutionHierarchy> Create(InstitutionHierarchy entity, List<long> directionsIds);

    Task<InstitutionHierarchy> Update(InstitutionHierarchy entity, List<long> directionsIds);
}