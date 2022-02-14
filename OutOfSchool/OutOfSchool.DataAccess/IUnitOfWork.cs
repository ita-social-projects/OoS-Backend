using System.Threading.Tasks;

namespace OutOfSchool.Services
{
    public interface IUnitOfWork
    {
        public Task<int> CompleteAsync();

        public int Complete();
    }
}