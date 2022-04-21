using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DapperRepository.DataMapping {
	public class PropertyMap {
		public string PropertyName { get; set; }
		public string ColumnName { get; set; }
		public bool IsMapped { get; set; }
		public bool IsKey { get; set; }
		public MethodInfo PropertyGetter { get; set; }
		public DatabaseGeneratedOption DatabaseGenerated { get; set; }
	}
}
