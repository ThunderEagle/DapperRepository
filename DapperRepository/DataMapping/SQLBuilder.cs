using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Composition;
using System.Linq;
using System.Text;
using ChangeTracking;
using Dapper;

namespace DapperRepository.DataMapping {
	[Export(typeof(ISQLBuilder<>))]
	public class SQLBuilder<T>:ISQLBuilder<T> where T : class {

		private string _columnList = string.Empty;
		private string _prefixedColumnList = string.Empty;
		private string _insertColumnList = string.Empty;

		[ImportingConstructor]
		public SQLBuilder(IEntityMapParser entityMapParser) {
			var type = typeof(T);
			EntityMap = entityMapParser.ParseMap(type);
		}


		public EntityMap EntityMap { get; }

		public string TableName => EntityMap?.TableName;

		public virtual string GetColumnList() {
			if(string.IsNullOrEmpty(_columnList)) {
				if(!EntityMap.PropertyMaps.Values.Any(IsMapped)) {
					throw new InvalidOperationException("No properties are mapped to the database.");
				}
				var sql = new StringBuilder();
				foreach(var map in EntityMap.PropertyMaps.Where(pm => IsMapped(pm.Value))) {
					sql.AppendFormat($"{map.Value.ColumnName},");
				}
				sql.Remove(sql.Length - 1, 1);
				_columnList = sql.ToString();
			}

			return _columnList;
		}

		public virtual string GetColumnList(string prefix) {
			if(!EntityMap.PropertyMaps.Values.Any(IsMapped)) {
				throw new InvalidOperationException("No properties are mapped to the database.");
			}
			var sql = new StringBuilder();
			foreach(var map in EntityMap.PropertyMaps.Where(pm => IsMapped(pm.Value))) {
				sql.AppendFormat($"{prefix}.{map.Value.ColumnName},");
			}
			sql.Remove(sql.Length - 1, 1);
			return sql.ToString();
		}

		public virtual string GetInsertColumnList() {
			if(string.IsNullOrEmpty(_insertColumnList)) {
				if(!EntityMap.PropertyMaps.Values.Any(IsInsertable)) {
					throw new InvalidOperationException("No properties are mapped as insertable to the database.");
				}
				var sql = new StringBuilder();
				foreach(var map in EntityMap.PropertyMaps.Where(pm => IsInsertable(pm.Value))) {
					sql.AppendFormat($"{map.Value.ColumnName},");
				}
				sql.Remove(sql.Length - 1, 1);
				_insertColumnList = sql.ToString();
			}

			return _insertColumnList;
		}

		public virtual string GetSelectStatement() {
			var sql = new StringBuilder("SELECT ");
			sql.Append(GetColumnList());
			sql.AppendFormat($" FROM {EntityMap.TableName}");
			return sql.ToString();
		}

		public virtual string GetSelectByIdStatement() {
			var sql = new StringBuilder("SELECT ");
			sql.Append(GetColumnList());
			sql.AppendFormat($" FROM {EntityMap.TableName}");
			sql.AppendFormat($" WHERE {EntityMap.KeyColumn} = @id");
			return sql.ToString();
		}

		protected virtual string GetDeclareTableVariableClause() {
			var sql = new StringBuilder("DECLARE @T TABLE (");
			var sqlType = GetKeySqlType();
			sql.AppendFormat($"{EntityMap.KeyColumn} {sqlType}");
			sql.AppendFormat(")");

			return sql.ToString();
		}

		protected virtual string GetKeySqlType() {
			var typeString = "INT";
			var type = EntityMap.KeyPropertyType;
			if(type == typeof(string)) {
				typeString = "NVARCHAR(MAX)";
			}
			else if(type == typeof(Int64)) {
				typeString = "BIGINT";
			}
			else if(type == typeof(Int32)) {
				typeString = "INT";
			}
			else if(type == typeof(Int16)) {
				typeString = "SMALLINT";
			}

			return typeString;

		}

		protected virtual string GetOutputIntoClause() {
			var sql = $"OUTPUT inserted.{EntityMap.KeyColumn} INTO @T";
			return sql;
		}

		protected virtual string GetSelectInsertedKeyClause() {
			var sql = $"SELECT {EntityMap.KeyColumn} FROM @T";
			return sql;
		}

		public virtual string GetInsertStatement(T entity, out DynamicParameters parms) {
			parms = new DynamicParameters();
			//var sql = new StringBuilder($"INSERT INTO {EntityMap.TableName} ({GetInsertColumnList()}) OUTPUT inserted.{EntityMap.KeyColumn} VALUES (");
			var sql = new StringBuilder(GetDeclareTableVariableClause());
			sql.AppendFormat($" INSERT INTO {EntityMap.TableName} ({GetInsertColumnList()})");
			sql.AppendFormat($" {GetOutputIntoClause()}");
			sql.AppendFormat($" VALUES (");

			foreach(var propertyMap in EntityMap.PropertyMaps.Where(pm => IsInsertable(pm.Value))) {
				sql.AppendFormat($"@{propertyMap.Value.PropertyName},");
				parms.Add($"@{propertyMap.Key}", propertyMap.Value.PropertyGetter.Invoke(entity, null));
			}
			sql.Remove(sql.Length - 1, 1);
			sql.Append(")");

			sql.AppendFormat($" {GetSelectInsertedKeyClause()}");

			return sql.ToString();
		}

		public virtual string GetUpdateStatement(T entity, out DynamicParameters parms) {

			if(!EntityMap.PropertyMaps.Values.Any(IsUpdatable)) {
				throw new InvalidOperationException("No properties are mapped as updatable to the database.");
			}
			var sql = new StringBuilder($"UPDATE {EntityMap.TableName} SET ");
			parms = new DynamicParameters();
			//only update what has changed
			if(entity is IChangeTrackable<T> trackable) {
				var changedUpdatableProperties = trackable.ChangedProperties.Where(cp => IsUpdatable(EntityMap.PropertyMaps[cp])).ToList();
				if(changedUpdatableProperties.Any()) {
					foreach(var propertyName in changedUpdatableProperties) {
						sql.AppendFormat($"{EntityMap.PropertyMaps[propertyName].ColumnName} = @{propertyName},");
						parms.Add($"@{propertyName}", EntityMap.PropertyMaps[propertyName].PropertyGetter.Invoke(entity, null));
					}
					sql.Remove(sql.Length - 1, 1);
				}
				else {
					return string.Empty;
				}
			}
			else {
				foreach(var propertyMap in EntityMap.PropertyMaps.Where(pm => IsUpdatable(pm.Value))) {
					sql.AppendFormat($"{propertyMap.Value.ColumnName} = @{propertyMap.Value.PropertyName},");
					parms.Add($"@{propertyMap.Key}", propertyMap.Value.PropertyGetter.Invoke(entity, null));
				}
				sql.Remove(sql.Length - 1, 1);
			}

			sql.AppendFormat($" WHERE {EntityMap.KeyColumn} = @{EntityMap.KeyProperty}");
			parms.Add($"@{EntityMap.KeyProperty}", EntityMap.PropertyMaps[EntityMap.KeyProperty].PropertyGetter.Invoke(entity, null));
			return sql.ToString();
		}

		protected virtual bool IsUpdatable(PropertyMap map) {
			return !map.IsKey && map.IsMapped && map.DatabaseGenerated == DatabaseGeneratedOption.None;
		}

		protected virtual bool IsInsertable(PropertyMap map) {
			return map.IsMapped && map.DatabaseGenerated == DatabaseGeneratedOption.None;
		}

		protected virtual bool IsMapped(PropertyMap map) {
			return map.IsMapped;
		}

		public virtual string GetDeleteByIdStatement() {
			return $"DELETE {EntityMap.TableName} WHERE {EntityMap.KeyColumn} = @id";
		}

		public virtual string GetDeleteStatement(T entity, out DynamicParameters parms) {
			var sql = $"DELETE {EntityMap.TableName} WHERE {EntityMap.KeyColumn} = @{EntityMap.KeyProperty}";
			parms = new DynamicParameters();
			parms.Add($"@{EntityMap.KeyProperty}", EntityMap.PropertyMaps[EntityMap.KeyProperty].PropertyGetter.Invoke(entity, null));
			return sql;
		}
	}
}
