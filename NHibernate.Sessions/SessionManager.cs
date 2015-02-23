using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Context;
using NHibernate.Util;
using Quarks;
using Quarks.NHibernate.ISessionFactoryExtensions;
using Remotion.Linq.Utilities;

namespace NHibernate.Sessions
{
	public class SessionManager : Disposable, ISessionManager
	{
		internal static readonly Dictionary<string, ISessionFactoryEntry> SessionFactoryEntries = new Dictionary<string, ISessionFactoryEntry>();

		const string noSessionFactoryEntries = "There are no session factory entries";

		protected override void DisposeManagedResources()
		{
			SessionFactoryEntries.Values
				.Where(x => x.IsInitialized)
				.ForEach(x =>
				{
					if (x.SessionFactory.HasCurrentSessionContext() && CurrentSessionContext.HasBind(x.SessionFactory))
						CurrentSessionContext.Unbind(x.SessionFactory).Close();
					x.SessionFactory.Close();
				});
			SessionFactoryEntries.Clear();
		}

		public void RegisterSessionFactory(ISessionFactoryEntry sessionFactoryEntry)
		{
			if (sessionFactoryEntry == null) throw new ArgumentNullException("sessionFactoryEntry");
			if (SessionFactoryEntries.ContainsKey(sessionFactoryEntry.Key))
				throw new InvalidOperationException(string.Format("A session factory entry with key '{0}' has already been added", sessionFactoryEntry.Key));

			SessionFactoryEntries.Add(sessionFactoryEntry.Key, sessionFactoryEntry);
		}

		public ISession Session
		{
			get
			{
				if (!SessionFactoryEntries.Any()) throw new InvalidOperationException(noSessionFactoryEntries);
				if (HasMultipleSessionFactories) throw new InvalidOperationException(getMultipleFactoryEntriesMessage("Session"));

				return SessionFactoryEntries.Values.Single().Session;
			}
		}

		public ISession SessionFor(string factoryKey)
		{
			if (!SessionFactoryEntries.Any()) throw new InvalidOperationException(noSessionFactoryEntries);
			if (string.IsNullOrWhiteSpace(factoryKey)) throw new ArgumentEmptyException("factoryKey");
			if (!SessionFactoryEntries.ContainsKey(factoryKey)) throw new ArgumentException(getFactoryKeyNotExistsMessage(factoryKey), "factoryKey");

			return SessionFactoryEntries[factoryKey].Session;
		}

		public ISession SessionFor<T>()
		{
			string key;
			return tryGetSessionFactoryKey<T>(out key)
				? SessionFor(key)
				: Session;
		}

		public ISessionFactory SessionFactory
		{
			get
			{
				if (!SessionFactoryEntries.Any()) throw new InvalidOperationException(noSessionFactoryEntries);
				if (HasMultipleSessionFactories) throw new InvalidOperationException(getMultipleFactoryEntriesMessage("SessionFactory"));

				return SessionFactoryEntries.Values.Single().SessionFactory;
			}
		}

		public ISessionFactory SessionFactoryFor(string factoryKey)
		{
			if (!SessionFactoryEntries.Any()) throw new InvalidOperationException(noSessionFactoryEntries);
			if (string.IsNullOrWhiteSpace(factoryKey)) throw new ArgumentEmptyException("factoryKey");
			if (!SessionFactoryEntries.ContainsKey(factoryKey)) throw new ArgumentException(getFactoryKeyNotExistsMessage(factoryKey), "factoryKey");

			return SessionFactoryEntries[factoryKey].SessionFactory;
		}

		public ISessionFactory SessionFactoryFor<T>()
		{
			string key;
			return tryGetSessionFactoryKey<T>(out key)
				? SessionFactoryFor(key)
				: SessionFactory;
		}

		public bool HasMultipleSessionFactories
		{
			get { return SessionFactoryEntries.Count > 1; }
		}

		static bool tryGetSessionFactoryKey<T>(out string key)
		{
			var attribute = AttributeHelper.GetTypeAttribute<T, SessionFactoryKeyAttribute>();
			key = attribute != null ? attribute.Key : null;
			return key != null;
		}

		static string getMultipleFactoryEntriesMessage(string memberName)
		{
			return string.Format("{0} may only be invoked if you have only one session factory. Since you've configured " +
								 "multiple session factories, you should instead call {0}For(string factoryKey) or " +
								 "{0}For<T>() where T is an entity decorated with a SessionFactoryKeyAttribute.", memberName);
		}

		static string getFactoryKeyNotExistsMessage(string factoryKey)
		{
			return string.Format("A session factory entry does not exist with a factory key of '{0}'", factoryKey);
		}
	}
}
