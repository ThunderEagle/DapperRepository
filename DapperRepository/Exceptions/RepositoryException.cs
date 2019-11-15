using System;
using System.Reflection;

namespace DapperRepository.Exceptions
{
	public class RepositoryException:Exception
	{
		public RepositoryException(MethodBase methodBase, Exception exception)
			: base(exception.Message, exception)
		{
			Method = methodBase.Name;
			Class = methodBase.DeclaringType != null ? methodBase.DeclaringType.FullName : null;
		}

		public string Class { get; }
		public string Method { get; }
	}
}
