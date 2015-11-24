using System;

namespace NHibernate.Sessions.Configuration
{
	public static class ConfigureNHibernate
	{
		public static ISingleSessionFactoryConfigurer ForSingleSessionFactory(Func<Cfg.Configuration> configurationProvider)
		{
			return new SingleSessionFactoryConfigurer(new KeylessSessionFactoryConfiguration(configurationProvider));
		}

		public static IMultiSessionFactoryConfigurer ForMultipleSessionFactories(Func<string, Cfg.Configuration> configurationFactory)
		{
			return new MultiSessionFactoryConfigurer(new KeyedSessionFactoryConfiguration(configurationFactory));
		}
	}
}
