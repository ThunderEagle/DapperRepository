using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Composition;
using System.Linq;
using System.Reflection;

namespace DapperRepository.DataMapping {
	[Export(typeof(IEntityMapParser))]
	public class EntityMapParser:IEntityMapParser {
		public EntityMap ParseMap(Type type) {
			var entityMap = new EntityMap() { PropertyMaps = new Dictionary<string, PropertyMap>() };

			//get the table
			var tableAttribute = type.GetCustomAttribute(typeof(TableAttribute), true) as TableAttribute;
			if(tableAttribute != null) {
				entityMap.TableName = tableAttribute.Name;
			}
			else {
				throw new InvalidOperationException("Must provide a Table name mapping.");
			}

			//walk through the properties and build the select statement
			var properties = type.GetProperties();
			foreach(var propertyInfo in properties) {
				var propertyMap = ParsePropertyMap(propertyInfo);
				entityMap.PropertyMaps.Add(propertyMap.PropertyName, propertyMap);
			}

			if(!entityMap.PropertyMaps.Any(pm => pm.Value.IsMapped)) {
				throw new InvalidOperationException("Must have at least one property mapped to a database column.");
			}

			if(!entityMap.PropertyMaps.All(pm => pm.Value.PropertyGetter.IsVirtual)) {
				throw new InvalidOperationException("All properties of entities must be declared virtual.");
			}

			try {
				var keyProperty = type.GetProperties().SingleOrDefault(prop => prop.GetCustomAttribute(typeof(KeyAttribute)) != null);
				if(keyProperty != null) {
					entityMap.KeyColumn = entityMap.PropertyMaps[keyProperty.Name].ColumnName;
					entityMap.KeyProperty = keyProperty.Name;
					entityMap.KeyPropertyType = keyProperty.PropertyType;
				}
			}
			catch(InvalidOperationException e) {
				throw new InvalidOperationException("Must map one key field.", e);
			}

			return entityMap;
		}

		private PropertyMap ParsePropertyMap(PropertyInfo property) {
			var columnAttribute = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
			var keyAttribute = property.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
			var databaseGeneratedAttribute = property.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) as DatabaseGeneratedAttribute;
			var notMappedAttribute = property.GetCustomAttribute(typeof(NotMappedAttribute)) as NotMappedAttribute;

			var propertyMap = new PropertyMap() {
				IsKey = (keyAttribute != null),
				PropertyGetter = property.GetGetMethod(),
				PropertyName = property.Name,
				IsMapped = (columnAttribute != null && notMappedAttribute == null)
			};

			if(columnAttribute != null) {
				propertyMap.ColumnName = columnAttribute.Name;
			}

			propertyMap.DatabaseGenerated = databaseGeneratedAttribute?.DatabaseGeneratedOption ?? DatabaseGeneratedOption.None;

			return propertyMap;
		}
	}
}