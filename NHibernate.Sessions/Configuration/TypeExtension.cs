using System.Linq;

namespace NHibernate.Sessions.Configuration
{
	public static class TypeExtension
	{
		public static bool IsForSessionFactory(this System.Type type, string sessionFactoryKey)
		{
			return type.GetInterfaces().Any(x => x.Name == sessionFactoryKey);
		}
	}
}