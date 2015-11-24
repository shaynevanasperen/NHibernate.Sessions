using System;

namespace NHibernate.Sessions.Configuration
{
	public class KeylessSessionFactoryConfiguration : SessionFactoryConfiguration, ISessionFactoryInitializion
	{
		static Func<Cfg.Configuration> _configurationProvider;

		public KeylessSessionFactoryConfiguration(Func<Cfg.Configuration> configurationProvider)
		{
			if (configurationProvider == null) throw new ArgumentNullException("configurationProvider");
			_configurationProvider = configurationProvider;
		}

		public virtual Func<Cfg.Configuration> ConfigurationProvider
		{
			get { return () => GetOrBuildConfiguration(GetType().Name, _configurationProvider); }
		}
	}
}