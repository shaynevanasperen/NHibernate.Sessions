using System;
using System.Threading.Tasks;
using Quarks.NHibernate.ISessionFactoryExtensions;
using Remotion.Linq.Utilities;

namespace NHibernate.Sessions
{
	public class SessionFactoryEntry : ISessionFactoryEntry
	{
		readonly Lazy<ISessionFactory> _sessionFactory;
		readonly Action<ISession> _onSessionOpened;

		internal Task SessionFactoryInitializationTask;

		public SessionFactoryEntry(string key, ISessionFactory sessionFactory,
			Action<ISessionFactory> onSessionFactoryInitialized = null, Action<ISession> onSessionOpened = null)
			: this(key, makeFunc(sessionFactory), SessionFactoryInitializationMode.Eager, onSessionFactoryInitialized, onSessionOpened) { }

		public SessionFactoryEntry(string key, Func<ISessionFactory> sessionFactoryProvider, SessionFactoryInitializationMode initializationMode,
			Action<ISessionFactory> onSessionFactoryInitialized = null, Action<ISession> onSessionOpened = null,
			Action<Exception> onSessionFactoryThreadedInitializationException = null)
		{
			if (string.IsNullOrWhiteSpace(key)) throw new ArgumentEmptyException("key");
			if (sessionFactoryProvider == null) throw new ArgumentNullException("sessionFactoryProvider");

			Key = key;
			_sessionFactory = new Lazy<ISessionFactory>(wrapSessionFactoryProvider(sessionFactoryProvider, onSessionFactoryInitialized));
			InitializationMode = initializationMode;
			_onSessionOpened = onSessionOpened;
			
			switch (InitializationMode)
			{
				case SessionFactoryInitializationMode.Eager:
					SessionFactory = _sessionFactory.Value;
					break;
				case SessionFactoryInitializationMode.Threaded:
					initializeSessionFactoryInAnotherThread(onSessionFactoryThreadedInitializationException);
					break;
			}
		}

		static Func<ISessionFactory> makeFunc(ISessionFactory sessionFactory)
		{
			if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
			return () => sessionFactory;
		}

		static Func<ISessionFactory> wrapSessionFactoryProvider(Func<ISessionFactory> sessionFactoryProvider, Action<ISessionFactory> onSessionFactoryInitialized)
		{
			return () =>
			{
				var sessionFactory = sessionFactoryProvider();
				if (onSessionFactoryInitialized != null)
					onSessionFactoryInitialized(sessionFactory);
				return sessionFactory;
			};
		}

		void initializeSessionFactoryInAnotherThread(Action<Exception> onSessionFactoryThreadedInitializationException)
		{
			SessionFactoryInitializationTask = Task.Factory.StartNew(() =>
			{
				try
				{
					SessionFactory = _sessionFactory.Value;
				}
				catch (Exception exception)
				{
					if (onSessionFactoryThreadedInitializationException == null)
						throw;
					onSessionFactoryThreadedInitializationException(new Exception(string
						.Format("Threaded Initialization: Unable to initialize NHibernate session factory with key '{0}'", Key), exception));
				}
			});
		}

		public string Key { get; protected set; }

		public SessionFactoryInitializationMode InitializationMode { get; private set; }

		ISessionFactory _initializedSessionFactory;
		public ISessionFactory SessionFactory
		{
			get { return _initializedSessionFactory ?? (_initializedSessionFactory = _sessionFactory.Value); }
			private set { _initializedSessionFactory = value; }
		}

		public bool IsInitialized
		{
			get { return _initializedSessionFactory != null; }
		}

		public virtual ISession Session
		{
			get
			{
				if (SessionFactory.HasCurrentSessionContext())
					return SessionFactory.GetCurrentOrNewSession(_onSessionOpened);

				var session = SessionFactory.OpenSession();
				if (_onSessionOpened != null)
					_onSessionOpened(session);
				return session;
			}
		}
	}
}
