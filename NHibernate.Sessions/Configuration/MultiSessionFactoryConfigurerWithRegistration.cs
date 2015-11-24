using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NHibernate.Sessions.Configuration
{
	public class MultiSessionFactoryConfigurerWithRegistration : IMultiSessionFactoryConfigurerWithRegistration
	{
		readonly IList<KeyedSessionFactoryConfiguration> _sessionFactoryConfigurations;
		KeyedSessionFactoryConfiguration _sessionFactoryConfiguration;
		readonly Lazy<Dictionary<string, ILazySessionFactory>> _lazySesionFactories;

		public MultiSessionFactoryConfigurerWithRegistration(IList<KeyedSessionFactoryConfiguration> sessionFactoryConfigurations,
			KeyedSessionFactoryConfiguration sessionFactoryConfiguration)
		{
			if (sessionFactoryConfigurations == null) throw new ArgumentNullException("sessionFactoryConfigurations");
			if (!sessionFactoryConfigurations.Any()) throw new ArgumentException("sessionFactoryConfigurations cannot be empty.", "sessionFactoryConfigurations");
			if (sessionFactoryConfiguration == null) throw new ArgumentNullException("sessionFactoryConfiguration");

			_sessionFactoryConfigurations = sessionFactoryConfigurations;
			_sessionFactoryConfiguration = sessionFactoryConfiguration;
			_lazySesionFactories = new Lazy<Dictionary<string, ILazySessionFactory>>(() =>
				_sessionFactoryConfigurations.ToDictionary(x => x.Key, x => (ILazySessionFactory)new LazySessionFactory(x)));
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithoutCachedConfiguration()
		{
			_sessionFactoryConfiguration.WithoutCachedConfiguration();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithCachedConfiguration()
		{
			_sessionFactoryConfiguration.WithCachedConfiguration();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithCachedConfiguration(params string[] dependentFilePaths)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(dependentFilePaths);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithCachedConfiguration(IConfigurationCache configurationCache, params string[] dependentFilePaths)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(configurationCache, dependentFilePaths);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithCachedConfiguration(params Assembly[] dependentAssemblies)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(dependentAssemblies);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithCachedConfiguration(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies)
		{
			_sessionFactoryConfiguration.WithCachedConfiguration(configurationCache, dependentAssemblies);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithEagerInitialization()
		{
			_sessionFactoryConfiguration.WithEagerInitialization();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithLazyInitialization()
		{
			_sessionFactoryConfiguration.WithLazyInitialization();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithBackgroundInitialization(Action<Exception> onBackgroundInitializationException = null)
		{
			_sessionFactoryConfiguration.WithBackgroundInitialization(onBackgroundInitializationException);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithInitializedCallback(Action<ISessionFactory> onInitialized)
		{
			_sessionFactoryConfiguration.WithInitializedCallback(onInitialized);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration WithSessionOpenedCallback(Action<ISession> onSessionOpened)
		{
			_sessionFactoryConfiguration.WithSessionOpenedCallback(onSessionOpened);
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration RegisterSessionFactory(string key)
		{
			_sessionFactoryConfiguration.Key = key;
			_sessionFactoryConfigurations.Add(_sessionFactoryConfiguration);
			_sessionFactoryConfiguration = _sessionFactoryConfiguration.ShallowCopy();
			return this;
		}

		public virtual IMultiSessionFactoryConfigurerWithRegistration RegisterSessionFactory<TFactoryKey>()
		{
			return RegisterSessionFactory(typeof(TFactoryKey).Name);
		}

		public virtual LazySessionFactories CreateLazySessionFactories()
		{
			return new LazySessionFactories(_lazySesionFactories.Value);
		}

		public virtual LazySessions CreateLazySessions()
		{
			var lazySessions = _sessionFactoryConfigurations.ToDictionary(x => x.Key, x => (ILazySessionScoper)new LazySession(_lazySesionFactories.Value[x.Key], x.OnSessionOpened));
			return new LazySessions(lazySessions);
		}
	}
}