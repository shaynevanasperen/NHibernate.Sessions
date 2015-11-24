using System;

namespace NHibernate.Sessions.Configuration
{
	public class KeyedSessionFactoryConfiguration : SessionFactoryConfiguration, ISessionFactoryInitializion
	{
		static Func<string, Cfg.Configuration> _configurationFactory;

		public KeyedSessionFactoryConfiguration(Func<string, Cfg.Configuration> configurationFactory)
		{
			if (configurationFactory == null) throw new ArgumentNullException("configurationFactory");
			_configurationFactory = configurationFactory;
		}

		KeyedSessionFactoryConfiguration() { }

		public virtual string Key { get; set; }

		public virtual Func<Cfg.Configuration> ConfigurationProvider
		{
			get { return () => GetOrBuildConfiguration(Key, () => _configurationFactory(Key)); }
		}

		public virtual KeyedSessionFactoryConfiguration ShallowCopy()
		{
			return new KeyedSessionFactoryConfiguration
			{
				ConfigurationCache = ConfigurationCache,
				CacheDependencyFilePaths = CacheDependencyFilePaths,
				InitializationMode = InitializationMode,
				OnBackgroundInitializationException = OnBackgroundInitializationException,
				OnInitialized = OnInitialized,
				OnSessionOpened = OnSessionOpened
			};
		}
	}
}