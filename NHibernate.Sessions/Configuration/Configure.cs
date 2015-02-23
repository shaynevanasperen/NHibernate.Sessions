using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Util;

namespace NHibernate.Sessions.Configuration
{
	public class Configure : ICompleteConfiguration,
		IConfigurationCacheSetting, IInitializationSetting, ISessionFactorySetting, ISessionSetting
	{
		public static IBeginConfiguration NHibernate
		{
			get { return new Configure(); }
		}

		readonly List<SessionFactoryRegistration> _sessionFactoryRegistrations = new List<SessionFactoryRegistration>();
		SessionFactoryRegistration _currentSessionFactoryRegistration = new SessionFactoryRegistration();

		readonly Lazy<IConfigurationCache> _defaultConfigurationCache = new Lazy<IConfigurationCache>(() =>
			new FileConfigurationCache(Assembly.GetExecutingAssembly().GetName(true).Name));

		Configure() { }

		public IPartialConfiguration Using(Func<string, Cfg.Configuration> configuration)
		{
			_currentSessionFactoryRegistration.Configuration = configuration;
			return this;
		}

		public IConfigurationCacheSetting WithConfiguration
		{
			get { return this; }
		}

		public IInitializationSetting WithInitialization
		{
			get { return this; }
		}

		public ISessionFactorySetting WithSessionFactory
		{
			get { return this; }
		}

		public ISessionSetting WithSession
		{
			get { return this; }
		}

		public IConfigurationCacheNegatedSetting Not
		{
			get { return new ConfigurationCacheNegatedSetting(this, _currentSessionFactoryRegistration); }
		}

		public IPartialConfiguration Cached()
		{
			_currentSessionFactoryRegistration.ConfigurationCache = _defaultConfigurationCache.Value;
			return this;
		}

		public IPartialConfiguration Cached(params string[] dependentFilePaths)
		{
			_currentSessionFactoryRegistration.ConfigurationCache = _currentSessionFactoryRegistration.ConfigurationCache ?? _defaultConfigurationCache.Value;
			_currentSessionFactoryRegistration.CacheDependencyFilePaths = dependentFilePaths;
			return this;
		}

		public IPartialConfiguration Cached(IConfigurationCache configurationCache, params string[] dependentFilePaths)
		{
			_currentSessionFactoryRegistration.ConfigurationCache = configurationCache;
			_currentSessionFactoryRegistration.CacheDependencyFilePaths = dependentFilePaths;
			return this;
		}

		public IPartialConfiguration Cached(params Assembly[] dependentAssemblies)
		{
			_currentSessionFactoryRegistration.ConfigurationCache = _currentSessionFactoryRegistration.ConfigurationCache ?? _defaultConfigurationCache.Value;
			_currentSessionFactoryRegistration.CacheDependencyFilePaths = DependentFilePaths.FromAssemblies(dependentAssemblies);
			return this;
		}

		public IPartialConfiguration Cached(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies)
		{
			_currentSessionFactoryRegistration.ConfigurationCache = configurationCache;
			_currentSessionFactoryRegistration.CacheDependencyFilePaths = DependentFilePaths.FromAssemblies(dependentAssemblies);
			return this;
		}

		public IPartialConfiguration Eager()
		{
			_currentSessionFactoryRegistration.SessionFactoryInitializationMode = SessionFactoryInitializationMode.Eager;
			return this;
		}

		public IPartialConfiguration Lazy()
		{
			_currentSessionFactoryRegistration.SessionFactoryInitializationMode = SessionFactoryInitializationMode.Lazy;
			return this;
		}

		public IPartialConfiguration Threaded(Action<Exception> onSessionFactoryThreadedInitializationException = null)
		{
			_currentSessionFactoryRegistration.SessionFactoryInitializationMode = SessionFactoryInitializationMode.Threaded;
			_currentSessionFactoryRegistration.OnSessionFactoryThreadedInitializationException = onSessionFactoryThreadedInitializationException;
			return this;
		}

		public IPartialConfiguration OnInitialized(Action<ISessionFactory> onSessionFactoryInitialized)
		{
			_currentSessionFactoryRegistration.OnSessionFactoryInitialized = onSessionFactoryInitialized;
			return this;
		}

		public IPartialConfiguration OnOpened(Action<ISession> onSessionOpened)
		{
			_currentSessionFactoryRegistration.OnSessionOpened = onSessionOpened;
			return this;
		}

		public ICompleteConfiguration RegisterSessionFactory(string sessionFactoryKey)
		{
			_currentSessionFactoryRegistration.SessionFactoryKey = sessionFactoryKey;
			_sessionFactoryRegistrations.Add(_currentSessionFactoryRegistration);
			_currentSessionFactoryRegistration = new SessionFactoryRegistration
			{
				Configuration = _currentSessionFactoryRegistration.Configuration,
				ConfigurationCache = _currentSessionFactoryRegistration.ConfigurationCache,
				CacheDependencyFilePaths = _currentSessionFactoryRegistration.CacheDependencyFilePaths,
				SessionFactoryInitializationMode = _currentSessionFactoryRegistration.SessionFactoryInitializationMode,
				OnSessionFactoryThreadedInitializationException = _currentSessionFactoryRegistration.OnSessionFactoryThreadedInitializationException,
				OnSessionFactoryInitialized = _currentSessionFactoryRegistration.OnSessionFactoryInitialized,
				OnSessionOpened = _currentSessionFactoryRegistration.OnSessionOpened
			};
			return this;
		}

		public ISessionManager BuildSessionManager()
		{
			var sessionManager = new SessionManager();
			_sessionFactoryRegistrations.Select(toSessionFactoryEntry).ForEach(sessionManager.RegisterSessionFactory);
			return sessionManager;
		}

		static SessionFactoryEntry toSessionFactoryEntry(SessionFactoryRegistration sessionFactoryRegistration)
		{
			return new SessionFactoryEntry(
				sessionFactoryRegistration.SessionFactoryKey,
				sessionFactoryRegistration.GetConfiguration().BuildSessionFactory,
				sessionFactoryRegistration.SessionFactoryInitializationMode,
				sessionFactoryRegistration.OnSessionFactoryInitialized,
				sessionFactoryRegistration.OnSessionOpened,
				sessionFactoryRegistration.OnSessionFactoryThreadedInitializationException);
		}
	}
}
