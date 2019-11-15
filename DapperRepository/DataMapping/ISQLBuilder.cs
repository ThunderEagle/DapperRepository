using Dapper;


namespace DapperRepository.DataMapping {
	public interface ISQLBuilder<T> where T : class {
		string GetColumnList();
		string GetColumnList(string prefix);
		string GetInsertColumnList();
		string GetSelectStatement();
		string GetSelectByIdStatement();
		string GetInsertStatement(T entity, out DynamicParameters parms);
		string GetUpdateStatement(T entity, out DynamicParameters parms);
		string GetDeleteByIdStatement();
		string GetDeleteStatement(T entity, out DynamicParameters parms);
		EntityMap EntityMap { get; }
		string TableName { get; }
	}
}