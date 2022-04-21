using System.Collections.Generic;

namespace DapperRepository {
	public interface IRepository<T, T1>:IReadOnlyRepository<T, T1> where T : class {
		T1 Insert(T itemToInsert);
		bool Update(T itemToUpdate);
		bool Update(IList<T> itemsToUpdate);
		bool Delete(T itemToDelete);
		bool Delete(T1 id);
		bool Delete(IEnumerable<T1> ids);
	}
}