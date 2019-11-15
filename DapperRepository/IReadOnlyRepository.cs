using System.Collections.Generic;
using System.Threading.Tasks;

namespace DapperRepository
{
    public interface IReadOnlyRepository<T, T1> where T : class
    {
        Task<T> GetAsync(T1 id);
        T Get(T1 id);
        Task<IList<T>> GetAllAsync();
        IList<T> GetAll();
    }
}