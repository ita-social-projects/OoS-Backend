using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Teacher entity.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private IEntityRepository<Teacher> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Teacher entity.</param>
        public TeacherService(IEntityRepository<Teacher> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Create(TeacherDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Teacher was null.");
            }

            try
            {
                var teacher = dto.ToDomain();

                var newTeacher = await repository.Create(teacher).ConfigureAwait(false);

                return newTeacher.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(TeacherDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAll()
        {
            try
            {
                var teachers = await repository.GetAll().ConfigureAwait(false);
                return teachers.Select(teacher => teacher.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve Teachers: {ex.Message}");
            }
        }

        public async Task<TeacherDTO> GetById(long id)
        {
            try
            {
                var teacher = await repository.GetById(id).ConfigureAwait(false);

                return teacher.ToModel();
            }
            catch (Exception e)
            {
                throw new Exception($"There is no {nameof(Teacher)} with id = {id}. {e.Message}");
            }
        }

        public async Task<TeacherDTO> Update(TeacherDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Teacher was null.");
            }

            try
            {
                var teacher = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return teacher.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(TeacherDTO)} could not be updated: {ex.Message}");
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                await repository
                    .Delete(await repository.GetById(id).ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }
        }
    }
}