using System.Linq;
using System.Reflection;

namespace NHibernate.Sessions.Configuration
{
	public static class DependentFilePaths
	{
		public static string[] FromAssemblies(params Assembly[] assemblies)
		{
			return assemblies.Select(x => x.GetName(true).Name).ToArray();
		}
	}
}
