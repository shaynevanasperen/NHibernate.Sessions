using System;
using System.Collections.Generic;
using System.Reflection;

namespace NHibernate.Sessions.Configuration
{
	public class MultiSessionFactoryConfigurer : IMultiSessionFactoryConfigurer
	{
		readonly IList<KeyedSessionFactoryConfiguration> _sessionFactoryConfigurations;
		readonly KeyedSessionFactoryConfiguration _sessionFactoryConfiguration;

		public MultiSessionFactoryConfigurer(KeyedSessionFactoryConfiguration sessionFactoryConfiguration)
		{
			if (sessionFactoryConfiguration == null) throw new ArgumentNullException("sessionFactoryConfiguration");
			_sessionFactoryConfigurations = new List<KeyedSessionFactoryConfiguration>();
			_sessionFactoryConfiguration = sessionFactoryConfiguration;
		}

		public virtual IMultiSessionFactoryConfigurer WithoutCachedConfiguration()
		{
			_sessionFactoryConfiguration.WithoutCachedConfiguration();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithCachedConfiguration()
		{
			_sessionFactoryConfiguration.WithCachedConfiguration();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithCachedConfiguration(params string[] dependentFilePaths)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(dependentFilePaths);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithCachedConfiguration(IConfigurationCache configurationCache, params string[] dependentFilePaths)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(configurationCache, dependentFilePaths);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithCachedConfiguration(params Assembly[] dependentAssemblies)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(dependentAssemblies);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithCachedConfiguration(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(configurationCache, dependentAssemblies);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithEagerInitialization()
		{
			_sessionFactoryConfiguration.WithEagerInitialization();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithLazyInitialization()
		{
			_sessionFactoryConfiguration.WithLazyInitialization();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithBackgroundInitialization(Action<Exception> onBackgroundInitializationException = null)
		{
			_sessionFactoryConfiguration.WithBackgroundInitialization(onBackgroundInitializationException);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithInitializedCallback(Action<ISessionFactory> onInitialized)
		{
			_sessionFactoryConfiguration.WithInitializedCallback(onInitialized);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurer WithSessionOpenedCallback(Action<ISession> onSessionOpened)
		{
			_sessionFactoryConfiguration.WithSessionOpenedCallback(onSessionOpened);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration RegisterSessionFactory(string key)
		{
			_sessionFactoryConfiguration.Key = key;
			_sessionFactoryConfigurations.Add(_sessionFactoryConfiguration);
			return new MultiSessionFactoryConfigurerWithRegistration(_sessionFactoryConfigurations, _sessionFactoryConfiguration.ShallowCopy());
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration RegisterSessionFactory<TFactoryKey>()
		{
			return RegisterSessionFactory(typeof(TFactoryKey).Name);
		}
	}
}