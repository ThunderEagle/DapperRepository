using System;
using System.Collections.Generic;

namespace DapperRepository.DataMapping {
	public class EntityMap {
		public string TableName { get; set; }
		public string KeyProperty { get; set; }
		public string KeyColumn { get; set; }
		public Type KeyPropertyType { get; set; }
		public Dictionary<string, PropertyMap> PropertyMaps { get; set; }
	}
}
