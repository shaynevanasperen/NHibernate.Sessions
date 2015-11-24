using System;
using System.Collections.Generic;
using NHibernate.Context;
using NHibernate.Engine;

namespace NHibernate.Sessions
{
	public static class SessionFactoryExtension
	{
		public static ISession GetCurrentOrNewSession(this ISessionFactory sessionFactory, ICurrentSessionContextBinder currentSessionContextBinder = null, Action<ISession> onSessionOpened = null)
		{
			currentSessionContextBinder = currentSessionContextBinder ?? sessionFactory.GetDefaultCurrentSessionContextBinder();
			if (currentSessionContextBinder.HasBind(sessionFactory))
				return sessionFactory.GetCurrentSession();
			var session = currentSessionContextBinder.BindNew(sessionFactory);
			if (onSessionOpened != null)
				onSessionOpened(session);
			return session;
		}

		static readonly IDictionary<ISessionFactory, ICurrentSessionContextBinder> _currentSessionContextBinders = new Dictionary<ISessionFactory, ICurrentSessionContextBinder>();
		public static ICurrentSessionContextBinder GetDefaultCurrentSessionContextBinder(this ISessionFactory sessionFactory)
		{
			if (_currentSessionContextBinders.ContainsKey(sessionFactory))
				return _currentSessionContextBinders[sessionFactory];

			var sessionFactoryImplementor = sessionFactory as ISessionFactoryImplementor;
			if (sessionFactoryImplementor == null)
				throw new HibernateException("Session factory does not implement ISessionFactoryImplementor.");
			if (sessionFactoryImplementor.CurrentSessionContext == null)
				throw new HibernateException("No current session context configured.");

			if (sessionFactoryImplementor.CurrentSessionContext is CurrentSessionContext)
			{
				_currentSessionContextBinders.Add(sessionFactory, CurrentSessionContextBinder.Instance);
				return _currentSessionContextBinders[sessionFactory];
			}
			if (sessionFactoryImplementor.CurrentSessionContext is ThreadLocalSessionContext)
			{
				_currentSessionContextBinders.Add(sessionFactory, ThreadLocalSessionContextBinder.Instance);
				return _currentSessionContextBinders[sessionFactory];
			}

			throw new HibernateException("Could not provide a default ICurrentSessionContextBinder " +
										 "as the configured current session context is not compatible " +
										 "with either CurrentSessionContext or ThreadLocalSessionContext.");
		}
	}
}
