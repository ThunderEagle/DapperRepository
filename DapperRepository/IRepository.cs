using System.Collections.Generic;

namespace DapperRepository {
	public interface IRepository<T>:IReadOnlyRepository<T> where T : class {
		int Insert(T itemToInsert);
		bool Update(T itemToUpdate);
		bool Update(IList<T> itemsToUpdate);
		bool Delete(T itemToDelete);
		bool Delete(int id);
		bool Delete(IEnumerable<int> ids);
	}
}