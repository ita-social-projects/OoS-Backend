using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for Teacher model.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private IEntityRepository<Teacher> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Teacher entity.</param>
        public TeacherService(IEntityRepository<Teacher> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Create(TeacherDTO teacherDto)
        {
            if (teacherDto == null)
            {
                throw new ArgumentNullException($"{nameof(TeacherDTO)} entity must not be null");
            }

            try
            {
                var teacher = teacherDto.ToDomain();

                var newTeacher = await _repository.Create(teacher).ConfigureAwait(false);

                return newTeacher.ToModel();
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
                var teachers = await _repository.GetAll().ConfigureAwait(false);
                var teachersDto = teachers.Select(teacher => teacher.ToModel());

                return teachersDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve Teachers: {ex.Message}");
            }
        }

        public async Task<TeacherDTO> GetById(long id)
        {
            var teacher = await _repository.GetById(id).ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentNullException($"There is no {nameof(teacher)} with id = {id}.");
            }

            return teacher.ToModel();
        }

        public async Task<TeacherDTO> Update(TeacherDTO teacherDto)
        {
            if (teacherDto == null)
            {
                throw new ArgumentNullException($"{nameof(teacherDto)} was null.");
            }

            try
            {
                var teacher = await _repository.Update(teacherDto.ToDomain()).ConfigureAwait(false);
              
                return teacher.ToModel();
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

            await _repository
                .Delete(teacherDto.ToDomain())
                .ConfigureAwait(false);
        }
    }
}