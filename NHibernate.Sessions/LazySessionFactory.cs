using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Sessions.Configuration;

namespace NHibernate.Sessions
{
	public class LazySessionFactory : ILazySessionFactory
	{
		internal Task ValueCreationTask;

		readonly Lazy<ISessionFactory> _sessionFactory;
		readonly Lazy<ICurrentSessionContextBinder> _defaultCurrentSessionContextBinder;

		public LazySessionFactory(ISessionFactoryInitializion sessionFactoryInitializion)
		{
			if (sessionFactoryInitializion == null) throw new ArgumentNullException("sessionFactoryInitializion");
			if (sessionFactoryInitializion.ConfigurationProvider == null) throw new ArgumentException("ConfigurationProvider cannot be null.", "sessionFactoryInitializion");

			_sessionFactory = new Lazy<ISessionFactory>(() => InitializeSessionFactory(sessionFactoryInitializion.ConfigurationProvider, sessionFactoryInitializion.OnInitialized));
			_defaultCurrentSessionContextBinder = new Lazy<ICurrentSessionContextBinder>(() => GetCurrentSessionContextBinder(Value));

			switch (sessionFactoryInitializion.InitializationMode)
			{
				case SessionFactoryInitializationMode.Eager:
					Value = _sessionFactory.Value;
					break;
				case SessionFactoryInitializationMode.Background:
					initializeInBackground(sessionFactoryInitializion);
					break;
			}
		}

		ICurrentSessionContextBinder _currentSessionContextBinder;
		public virtual ICurrentSessionContextBinder CurrentSessionContextBinder
		{
			get
			{
				// Ensure initialized so that if we were configured to be lazy, the initialization gets
				// a chance to set the current session context binder specified by configuration
				Value = _sessionFactory.Value;
				return _currentSessionContextBinder ?? _defaultCurrentSessionContextBinder.Value;
			}
		}

		protected virtual ISessionFactory InitializeSessionFactory(Func<Cfg.Configuration> configurationProvider, Action<ISessionFactory> onInitialized)
		{
			var configuration = configurationProvider();
			_currentSessionContextBinder = GetCurrentSessionContextBinder(configuration);
			var sessionFactory = configuration.BuildSessionFactory();
			if (onInitialized != null)
				onInitialized(sessionFactory);
			return sessionFactory;
		}

		protected virtual ICurrentSessionContextBinder GetCurrentSessionContextBinder(Cfg.Configuration configuration)
		{
			if (!configuration.Properties.ContainsKey(ConfigurationExtension.CurrentSessionContextBinderClass))
				return null;

			var currentSessionContextBinderTypeName = configuration.Properties[ConfigurationExtension.CurrentSessionContextBinderClass];
			var currentSessionContextBinderType = System.Type.GetType(currentSessionContextBinderTypeName);
			if (currentSessionContextBinderType == null)
				throw new TypeLoadException(string.Format("'{0}' is not a valid type name.", currentSessionContextBinderTypeName));
			var currentSessionContextBinder = Activator.CreateInstance(currentSessionContextBinderType) as ICurrentSessionContextBinder;
			if (currentSessionContextBinder == null)
				throw new Exception(string.Format("The type specified by the '{0}' property does not implement ICurrentSessionContextBinder.",
					ConfigurationExtension.CurrentSessionContextBinderClass));
			return currentSessionContextBinder;
		}

		protected virtual ICurrentSessionContextBinder GetCurrentSessionContextBinder(ISessionFactory sessionFactory)
		{
			return sessionFactory.GetDefaultCurrentSessionContextBinder();
		}

		void initializeInBackground(ISessionFactoryInitializion sessionFactoryInitializion)
		{
			ValueCreationTask = Task.Factory.StartNew(() =>
			{
				try
				{
					if (sessionFactoryInitializion.OnBackgroundInitializationStarted != null)
						sessionFactoryInitializion.OnBackgroundInitializationStarted(Thread.CurrentThread);
					Value = _sessionFactory.Value;
					if (sessionFactoryInitializion.OnBackgroundInitializationCompleted != null)
						sessionFactoryInitializion.OnBackgroundInitializationCompleted(Thread.CurrentThread);
				}
				catch (Exception exception)
				{
					if (sessionFactoryInitializion.OnBackgroundInitializationException == null)
						throw;
					sessionFactoryInitializion.OnBackgroundInitializationException(exception);
				}
			}, TaskCreationOptions.LongRunning);
		}

		public virtual bool IsValueCreated
		{
			get { return _sessionFactory.IsValueCreated; }
		}

		ISessionFactory _value;
		public virtual ISessionFactory Value
		{
			get { return _value ?? (_value = _sessionFactory.Value); }
			private set { _value = value; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool _disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing && IsValueCreated)
				Value.Dispose();
			_disposed = true;
		}
	}
}
