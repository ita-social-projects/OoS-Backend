using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Services.Implementation
{
    /// <summary>
    /// Service with business logic for Teacher model.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private IEntityRepository<Teacher> TeacherRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="mapper">Mapper.</param>
        /// <param name="teacherRepository">Repository for Teacher entity.</param>
        public TeacherService(IMapper mapper, IEntityRepository<Teacher> teacherRepository)
        {
            this.mapper = mapper;
            TeacherRepository = teacherRepository;
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Create(TeacherDTO teacher)
        {
            if (teacher == null)
            {
                throw new ArgumentNullException($"{nameof(TeacherDTO)} entity must not be null");
            }

            try
            {
                var newTeacher = mapper.Map<TeacherDTO, Teacher>(teacher);

                await TeacherRepository.Create(newTeacher).ConfigureAwait(false);

                return teacher;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(TeacherDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TeacherDTO> GetAllTeachers()
        {
            IEnumerable<TeacherDTO> teacherDto = TeacherRepository.GetAll().Select(
                teacher => mapper.Map<Teacher, TeacherDTO>(teacher));

            return teacherDto;
        }
    }
}