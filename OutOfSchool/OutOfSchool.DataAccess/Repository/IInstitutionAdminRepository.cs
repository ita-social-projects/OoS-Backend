using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IInstitutionAdminRepository : IInstitutionAdminRepositoryBase<Guid, InstitutionAdmin>
{
}