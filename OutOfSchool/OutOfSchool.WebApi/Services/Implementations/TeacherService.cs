using System;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Services.Implementations
{
    public class TeacherService : ITeacherService
    {
        private readonly IMapper _mapper;
        private readonly OutOfSchoolDbContext _context;

        public TeacherService(OutOfSchoolDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Teacher> CreateAsync(TeacherDto teacherDto)
        {
            if (teacherDto == null)
            {
                throw new ArgumentNullException($"{nameof(TeacherDto)} entity must not be null");
            }

            try
            {
                var teacher = _mapper.Map<TeacherDto, Teacher>(teacherDto);

                await _context.Teachers.AddAsync(teacher);
                await _context.SaveChangesAsync();

                return teacher;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(TeacherDto)} could not be saved: {ex.Message}");
            }
        }
    }
}