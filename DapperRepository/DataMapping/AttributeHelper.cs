using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Dapper;

namespace DapperRepository.DataMapping
{
	public static class AttributeHelper
	{
		public static string GetColumnNameFromAttribute(MemberInfo member)
		{
			if(member == null)
			{
				return null;
			}

			var attrib = (ColumnAttribute)Attribute.GetCustomAttribute(member, typeof(ColumnAttribute), false);
			return attrib?.Name;
		}

		public static void SetCustomMap(Type typeToMap)
		{
			var map = new CustomPropertyTypeMap(typeToMap,
												(type, columnName) => type.GetProperties().FirstOrDefault(prop => GetColumnNameFromAttribute(prop) == columnName));

			SqlMapper.SetTypeMap(typeToMap, map);
		}
	}
}