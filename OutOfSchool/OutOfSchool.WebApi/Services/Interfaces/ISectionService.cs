using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    public interface ISectionService
    {
        Task<SectionDTO> Create(SectionDTO section);
        IEnumerable<SectionDTO> GetAll();
    }
}