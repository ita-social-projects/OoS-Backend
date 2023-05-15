using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IAreaAdminRepository : IInstitutionAdminRepositoryBase<long, AreaAdmin>
{
}