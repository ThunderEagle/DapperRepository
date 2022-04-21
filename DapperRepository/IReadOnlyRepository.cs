using System.Collections.Generic;
using System.Threading.Tasks;

namespace DapperRepository {
	public interface IReadOnlyRepository<T> where T : class {
		Task<T> GetAsync(int id);
		T Get(int id);
		Task<IList<T>> GetAllAsync();
		IList<T> GetAll();
	}
}