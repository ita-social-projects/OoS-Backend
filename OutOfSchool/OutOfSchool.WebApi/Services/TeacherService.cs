using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Teacher entity.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly IEntityRepository<Teacher> repository;

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
            var teacher = dto.ToDomain();

            var newTeacher = await repository.Create(teacher).ConfigureAwait(false);

            return newTeacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAll()
        {
            var teachers = await repository.GetAll().ConfigureAwait(false);

            return teachers.Select(teacher => teacher.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> GetById(long id)
        {
            var teacher = await repository.GetById(id).ConfigureAwait(false);

            return teacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Update(TeacherDTO dto)
        {
            var teacher = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

            return teacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            var dtoToDelete = new Teacher() {Id = id};

            await repository.Delete(dtoToDelete).ConfigureAwait(false);
        }
    }
}