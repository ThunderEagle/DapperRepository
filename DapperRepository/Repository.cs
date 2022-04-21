using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ChangeTracking;
using Dapper;
using DapperRepository.DataMapping;
using DapperRepository.Exceptions;

namespace DapperRepository {
	public class Repository<T>:ReadOnlyRepository<T>, IRepository<T> where T : class {
		public Repository(IConnectionFactory connectionFactory, ISQLBuilder<T> sqlBuilder)
						: base(connectionFactory, sqlBuilder) { }

		public override async Task<T> GetAsync(int id) {
			var result = await base.GetAsync(id);
			return result?.AsTrackable();
		}

		public override T Get(int id) {
			return base.Get(id)?.AsTrackable();
		}

		public override async Task<IList<T>> GetAllAsync() {
			var result = await base.GetAllAsync();
			return result.AsTrackable();
		}

		public override IList<T> GetAll() {
			return base.GetAll().AsTrackable();
		}

		public virtual int Insert(T itemToInsert) {
			try {
				var sql = SqlBuilder.GetInsertStatement(itemToInsert, out var parms);

				using(var cn = ConnectionFactory.Invoke()) {
					var key = cn.ExecuteScalar<int>(sql, parms);
					(itemToInsert as IChangeTrackable)?.AcceptChanges();
					return key;
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual bool Update(T itemToUpdate) {
			try {
				var sql = SqlBuilder.GetUpdateStatement(itemToUpdate, out var parms);
				if(!string.IsNullOrEmpty(sql)) {
					using(var cn = ConnectionFactory.Invoke()) {
						var rowCount = cn.Execute(sql, parms);
						if(rowCount > 0) {
							((IChangeTrackable)itemToUpdate)?.AcceptChanges();
						}
						return rowCount == 1;
					}
				}
				return true;
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual bool Update(IList<T> itemsToUpdate) {
			var exceptions = new List<Exception>();

			using(var transaction = TransactionScopeHelper.CreateNew()) {
				if(itemsToUpdate is IChangeTrackableCollection<T> trackableCollection) {
					if(!trackableCollection.IsChanged) {
						throw new InvalidOperationException("List has not been changed!");
					}

					foreach(var item in trackableCollection.AddedItems) {
						try {
							Insert(item);
						}
						catch(Exception e) {
							exceptions.Add(e);
						}
					}

					foreach(var item in trackableCollection.DeletedItems) {
						try {
							Delete(item);
						}
						catch(Exception e) {
							exceptions.Add(e);
						}
					}

					foreach(var item in trackableCollection.ChangedItems) {
						try {
							Update(item);
						}
						catch(Exception e) {
							exceptions.Add(e);
						}
					}
				}
				else {
					foreach(var item in itemsToUpdate) {
						try {
							Update(item);
						}
						catch(Exception e) {
							exceptions.Add(e);
						}
					}
				}

				if(exceptions.Any()) {
					throw new AggregateException(exceptions);
				}

				transaction.Complete();
				return true;
			}
		}

		public virtual bool Delete(T itemToDelete) {
			try {
				var sql = SqlBuilder.GetDeleteStatement(itemToDelete, out var parms);
				using(var cn = ConnectionFactory.Invoke()) {
					return cn.Execute(sql, parms) == 1;
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual bool Delete(int id) {
			try {
				var sql = SqlBuilder.GetDeleteByIdStatement();
				using(var cn = ConnectionFactory.Invoke()) {
					var parms = new DynamicParameters(new { id });
					return cn.Execute(sql, parms) == 1;
				}
			}
			catch(RepositoryException) {
				throw;
			}
			catch(Exception e) {
				throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
			}
		}

		public virtual bool Delete(IEnumerable<int> ids) {
			bool isSuccessful = true;
			using(var transaction = TransactionScopeHelper.CreateNew()) {
				var exceptions = new List<Exception>();
				try {
					foreach(var id in ids) {
						var result = Delete(id);
						if(!result) {
							isSuccessful = false;
						}
					}
				}
				catch(Exception e) {
					exceptions.Add(e);
				}

				if(exceptions.Any()) {
					throw new AggregateException(exceptions);
				}
				transaction.Complete();
			}
			return isSuccessful;
		}
	}
}