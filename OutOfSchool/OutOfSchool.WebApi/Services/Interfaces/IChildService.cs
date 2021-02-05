using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    public interface IChildService
    {
        Task<ChildDTO> Create(ChildDTO child);
    }
}
