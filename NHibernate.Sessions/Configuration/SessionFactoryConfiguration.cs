using System;
using System.Reflection;
using System.Threading;

namespace NHibernate.Sessions.Configuration
{
	public abstract class SessionFactoryConfiguration
	{
		readonly Lazy<IConfigurationCache> _defaultConfigurationCache = new Lazy<IConfigurationCache>(() =>
			new FileConfigurationCache(Assembly.GetExecutingAssembly().GetName(true).Name));

		protected virtual IConfigurationCache ConfigurationCache { get; set; }
		protected virtual string[] CacheDependencyFilePaths { get; set; }

		public virtual SessionFactoryInitializationMode InitializationMode { get; protected set; }
		public virtual Action<Exception> OnBackgroundInitializationException { get; protected set; }
		public virtual Action<Thread> OnBackgroundInitializationStarted { get; protected set; }
		public virtual Action<Thread> OnBackgroundInitializationCompleted { get; protected set; }
		public virtual Action<ISessionFactory> OnInitialized { get; protected set; }

		public virtual Action<ISession> OnSessionOpened { get; protected set; }

		public virtual void WithoutCachedConfiguration()
		{
			ConfigurationCache = null;
		}

		public virtual void WithCachedConfiguration()
		{
			ConfigurationCache = _defaultConfigurationCache.Value;
		}

		public virtual void WithCachedConfiguration(params string[] dependentFilePaths)
		{
			ConfigurationCache = ConfigurationCache ?? _defaultConfigurationCache.Value;
			CacheDependencyFilePaths = dependentFilePaths;
		}

		public virtual void WithCachedConfiguration(IConfigurationCache configurationCache, params string[] dependentFilePaths)
		{
			ConfigurationCache = configurationCache;
			CacheDependencyFilePaths = dependentFilePaths;
		}

		public virtual void WithCachedConfiguration(params Assembly[] dependentAssemblies)
		{
			ConfigurationCache = ConfigurationCache ?? _defaultConfigurationCache.Value;
			CacheDependencyFilePaths = DependentFilePaths.FromAssemblies(dependentAssemblies);
		}

		public virtual void WithCachedConfiguration(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies)
		{
			ConfigurationCache = configurationCache;
			CacheDependencyFilePaths = DependentFilePaths.FromAssemblies(dependentAssemblies);
		}

		public virtual void WithEagerInitialization()
		{
			InitializationMode = SessionFactoryInitializationMode.Eager;
		}

		public virtual void WithLazyInitialization()
		{
			InitializationMode = SessionFactoryInitializationMode.Lazy;
		}

		public virtual void WithBackgroundInitialization(Action<Exception> onBackgroundInitializationException = null,
			Action<Thread> onBackgroundInitializationStarted = null, Action<Thread> onBackgroundInitializationCompleted = null)
		{
			InitializationMode = SessionFactoryInitializationMode.Background;
			OnBackgroundInitializationException = onBackgroundInitializationException;
			OnBackgroundInitializationStarted = onBackgroundInitializationStarted;
			OnBackgroundInitializationCompleted = onBackgroundInitializationCompleted;
		}

		public virtual void WithInitializedCallback(Action<ISessionFactory> onInitialized)
		{
			OnInitialized = onInitialized;
		}

		public virtual void WithSessionOpenedCallback(Action<ISession> onSessionOpened)
		{
			OnSessionOpened = onSessionOpened;
		}

		protected virtual Cfg.Configuration GetOrBuildConfiguration(string configKey, Func<Cfg.Configuration> buildConfiguration)
		{
			if (ConfigurationCache == null)
				return buildConfiguration();

			var configuration = ConfigurationCache.LoadConfiguration(configKey, CacheDependencyFilePaths);

			if (configuration != null)
				return configuration;

			configuration = buildConfiguration();
			ConfigurationCache.SaveConfiguration(configKey, configuration);

			return configuration;
		}
	}
}