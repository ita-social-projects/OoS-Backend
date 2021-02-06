using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public class ChildService : IChildService
    {
        private IEntityRepository<Child> _ChildRepository { get; set; }
        private readonly IMapper mapper;

        public ChildService(IEntityRepository<Child> entityRepository, IMapper mapper)
        {
            _ChildRepository = entityRepository;
            this.mapper = mapper;
        }
        public async Task<ChildDTO> Create(ChildDTO child)
        {
            if (child.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Invalid Date of birth");
            }
            Child newChild = mapper.Map<ChildDTO, Child>(child);
            var child_ = await _ChildRepository.Create(newChild);
            return await Task.FromResult(mapper.Map<Child, ChildDTO>(child_));
        }
        
    }
}
