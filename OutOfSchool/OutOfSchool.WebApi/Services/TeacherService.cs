using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
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

                var teacher_ = await TeacherRepository.Create(newTeacher).ConfigureAwait(false);

                return mapper.Map<Teacher, TeacherDTO>(teacher_);
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(TeacherDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAllTeachers()
        {
            try
            {
                var teachers = await Task.Run(() =>
                        TeacherRepository.GetAll().Result
                            .Select(teacher => mapper.Map<Teacher, TeacherDTO>(teacher)))
                    .ConfigureAwait(false);

                return teachers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve Teachers: {ex.Message}");
            }
        }

        public async Task<TeacherDTO> GetById(long id)
        {
            var teacher = await TeacherRepository.GetById(id).ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentNullException($"There is no {nameof(teacher)} with id = {id}.");
            }

            return mapper.Map<Teacher, TeacherDTO>(teacher);
        }

        public async Task<TeacherDTO> Update(TeacherDTO teacher)
        {
            if (teacher == null)
            {
                throw new ArgumentNullException($"{nameof(teacher)} was null.");
            }

            try
            {
                await TeacherRepository.Update(mapper.Map<TeacherDTO, Teacher>(teacher)).ConfigureAwait(false);

                return mapper.Map<Teacher, TeacherDTO>(await TeacherRepository
                    .Update(mapper.Map<TeacherDTO, Teacher>(teacher))
                    .ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(TeacherDTO)} could not be updated: {ex.Message}");
            }
        }

        public async Task Delete(long id)
        {
            TeacherDTO teacherDto;

            try
            {
                teacherDto = await GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            await TeacherRepository
                .Delete(mapper.Map<TeacherDTO, Teacher>(teacherDto))
                .ConfigureAwait(false);
        }
    }
}