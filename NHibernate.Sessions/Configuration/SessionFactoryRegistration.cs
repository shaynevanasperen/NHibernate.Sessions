using System;

namespace NHibernate.Sessions.Configuration
{
	class SessionFactoryRegistration
	{
		internal Func<string, Cfg.Configuration> Configuration { get; set; }
		internal IConfigurationCache ConfigurationCache { get; set; }
		internal string[] CacheDependencyFilePaths { get; set; }
		internal SessionFactoryInitializationMode SessionFactoryInitializationMode { get; set; }
		internal Action<Exception> OnSessionFactoryThreadedInitializationException { get; set; }
		internal Action<ISessionFactory> OnSessionFactoryInitialized { get; set; }
		internal Action<ISession> OnSessionOpened { get; set; }
		internal string SessionFactoryKey { get; set; }

		internal Cfg.Configuration GetConfiguration()
		{
			if (ConfigurationCache == null)
				return buildConfiguration();

			var configuration = ConfigurationCache.LoadConfiguration(SessionFactoryKey, CacheDependencyFilePaths);

			if (configuration != null)
				return configuration;

			configuration = buildConfiguration();
			ConfigurationCache.SaveConfiguration(SessionFactoryKey, configuration);

			return configuration;
		}

		Cfg.Configuration buildConfiguration()
		{
			return Configuration(SessionFactoryKey);
		}
	}
}