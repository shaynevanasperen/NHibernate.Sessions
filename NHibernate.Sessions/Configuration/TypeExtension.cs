using Quarks;

namespace NHibernate.Sessions.Configuration
{
	public static class TypeExtension
	{
		public static bool IsForSessionFactory(this System.Type type, string sessionFactoryKey)
		{
			var attribute = AttributeHelper.GetTypeAttribute<SessionFactoryKeyAttribute>(type);
			return attribute == null || attribute.Key == sessionFactoryKey;
		}
	}
}