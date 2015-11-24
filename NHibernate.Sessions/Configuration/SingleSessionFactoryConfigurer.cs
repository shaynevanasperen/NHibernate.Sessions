using System;
using System.Reflection;
using System.Threading;

namespace NHibernate.Sessions.Configuration
{
	public class SingleSessionFactoryConfigurer : ISingleSessionFactoryConfigurer
	{
		readonly KeylessSessionFactoryConfiguration _sessionFactoryConfiguration;
		readonly Lazy<LazySessionFactory> _lazySessionFactory;

		public SingleSessionFactoryConfigurer(KeylessSessionFactoryConfiguration sessionFactoryConfiguration)
		{
			if (sessionFactoryConfiguration == null) throw new ArgumentNullException("sessionFactoryConfiguration");
			_sessionFactoryConfiguration = sessionFactoryConfiguration;
			_lazySessionFactory = new Lazy<LazySessionFactory>(() => new LazySessionFactory(_sessionFactoryConfiguration));
		}

		public virtual ISingleSessionFactoryConfigurer WithoutCachedConfiguration()
		{
			_sessionFactoryConfiguration.WithoutCachedConfiguration();
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithCachedConfiguration()
		{
			_sessionFactoryConfiguration.WithCachedConfiguration();
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithCachedConfiguration(params string[] dependentFilePaths)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(dependentFilePaths);
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithCachedConfiguration(IConfigurationCache configurationCache, params string[] dependentFilePaths)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(configurationCache, dependentFilePaths);
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithCachedConfiguration(params Assembly[] dependentAssemblies)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(dependentAssemblies);
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithCachedConfiguration(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(configurationCache, dependentAssemblies);
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithEagerInitialization()
		{
			_sessionFactoryConfiguration.WithEagerInitialization();
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithLazyInitialization()
		{
			_sessionFactoryConfiguration.WithLazyInitialization();
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithBackgroundInitialization(Action<Exception> onBackgroundInitializationException = null,
			Action<Thread> onBackgroundInitializationStarted = null, Action<Thread> onBackgroundInitializationCompleted = null)
		{
			_sessionFactoryConfiguration.WithBackgroundInitialization(onBackgroundInitializationException, onBackgroundInitializationStarted, onBackgroundInitializationCompleted);
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithInitializedCallback(Action<ISessionFactory> onInitialized)
		{
			_sessionFactoryConfiguration.WithInitializedCallback(onInitialized);
			return this;
		}

		public virtual ISingleSessionFactoryConfigurer WithSessionOpenedCallback(Action<ISession> onSessionOpened)
		{
			_sessionFactoryConfiguration.WithSessionOpenedCallback(onSessionOpened);
			return this;
		}

		public virtual LazySessionFactory CreateLazySessionFactory()
		{
			return _lazySessionFactory.Value;
		}

		public virtual LazySession CreateLazySession()
		{
			return new LazySession(_lazySessionFactory.Value, _sessionFactoryConfiguration.OnSessionOpened);
		}
	}
}