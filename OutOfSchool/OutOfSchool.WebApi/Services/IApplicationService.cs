using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public interface IApplicationService
    {
        Task<ApplicationDTO> Create(ApplicationDTO application);

        Task<IEnumerable<ApplicationDTO>> GetAll();

        Task<ApplicationDTO> GetById(long id);

        Task<IEnumerable<ApplicationDTO>> GetAllByWorkshop(long id);

        Task<IEnumerable<ApplicationDTO>> GetAllByUser(string id);

        Task<ApplicationDTO> Update(ApplicationDTO application);

        Task Delete(long id);
    }
}
