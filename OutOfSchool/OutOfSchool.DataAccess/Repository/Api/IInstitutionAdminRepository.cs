using System;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Api;

public interface IInstitutionAdminRepository : IInstitutionAdminRepositoryBase<Guid, InstitutionAdmin>
{
}