using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public interface IEntityRepository<T> where T : class, new()
    {
        Task<T> Create(T entity);
        void Update(T entity);
        Task Delete(T entity);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllWithDetails(string includeProperties = "");
        Task<T> GetById(long id);
    }
}
