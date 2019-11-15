using System;

namespace DapperRepository.DataMapping {
	public interface IEntityMapParser {
		EntityMap ParseMap(Type type);
	}
}