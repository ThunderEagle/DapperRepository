using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using DapperRepository.DataMapping;
using DapperRepository.Exceptions;

namespace DapperRepository {
	public class ReadOnlyRepository<T>:IReadOnlyRepository<T> where T : class {
		public ReadOnlyRepository(IConnectionFactory connectionFactory, ISQLBuilder<T> sqlBuilder) {
			ConnectionFactory = connectionFactory.GetConnection;
			// custom mapping
			var map = new CustomPropertyTypeMap(typeof(T),
					(type, columnName) => type.GetProperties().FirstOrDefault(prop => AttributeHelper.GetColumnNameFromAttribute(prop) == columnName));
			SqlMapper.SetTypeMap(typeof(T), map);
			SqlBuilder = sqlBuilder;
		}

		protected ISQLBuilder<T> SqlBuilder { get; }
		protected Func<IDbConnection> ConnectionFactory { get; }

		public virtual async Task<T> GetAsync(int id) {
			try {
				var sql = SqlBuilder.GetSelectByIdStatement();
				var parms = new DynamicParameters(new { id });
				using(var cn = ConnectionFactory.Invoke()) {
					return (await cn.QueryFirstOrDefaultAsync<T>(sql, parms));
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual T Get(int id) {
			try {
				var sql = SqlBuilder.GetSelectByIdStatement();
				var parms = new DynamicParameters(new { id });
				using(var cn = ConnectionFactory.Invoke()) {
					return (cn.QueryFirstOrDefault<T>(sql, parms));
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual async Task<IList<T>> GetAllAsync() {
			try {
				var sql = SqlBuilder.GetSelectStatement();

				using(var cn = ConnectionFactory.Invoke()) {
					var result = await cn.QueryAsync<T>(sql);
					return result != null ? result.ToList() : new List<T>();
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual IList<T> GetAll() {
			try {
				var sql = SqlBuilder.GetSelectStatement();

				using(var cn = ConnectionFactory.Invoke()) {
					var result = cn.Query<T>(sql);
					return result != null ? result.ToList() : new List<T>();
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

	}
}
