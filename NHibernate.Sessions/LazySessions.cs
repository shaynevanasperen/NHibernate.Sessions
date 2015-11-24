using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Context;

namespace NHibernate.Sessions
{
	/// <summary>
	/// Warning: This won't work if you're using <see cref="ThreadStaticSessionContext"/> as your current session context class.
	/// </summary>
	public class LazySessions : ILazySessions, ILazySessionsScoper
	{
		readonly IDictionary<string, ILazySessionScoper> _lazySessionScopers;

		public LazySessions(IDictionary<string, ILazySessionScoper> lazySessionScopers)
		{
			if (lazySessionScopers == null) throw new ArgumentNullException("lazySessionScopers");
			if (!lazySessionScopers.Any()) throw new ArgumentException("lazySessions cannot be empty.", "lazySessionScopers");

			_lazySessionScopers = lazySessionScopers;
		}

		public virtual ISession CurrentFor(string factoryKey)
		{
			return ScopeFor(factoryKey).Current;
		}

		public virtual ISession CurrentFor<TFactoryKey>()
		{
			return CurrentFor(typeof(TFactoryKey).Name);
		}

		public virtual ILazySessionScope ScopeFor(string factoryKey)
		{
			return _lazySessionScopers[factoryKey].Scope();
		}

		public virtual ILazySessionScope ScopeFor<TFactoryKey>()
		{
			return ScopeFor(typeof(TFactoryKey).Name);
		}

		public virtual ILazySessionsScope Scope()
		{
			return new LazySessionsScope(this);
		}

		public virtual void UnbindAndDisposeCurrent()
		{
			foreach (var lazySessionScoper in _lazySessionScopers.Values)
				lazySessionScoper.UnbindAndDisposeCurrent();
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
			if (disposing)
			{
				foreach (var lazySession in _lazySessionScopers.Values)
					lazySession.Dispose();
				_lazySessionScopers.Clear();
			}
			_disposed = true;
		}
	}

	public class LazySessionsScope : ILazySessionsScope
	{
		readonly LazySessions _lazySessions;

		public LazySessionsScope(LazySessions lazySessions)
		{
			_lazySessions = lazySessions;
		}

		public ILazySessionScope ScopeFor(string factoryKey)
		{
			return _lazySessions.ScopeFor(factoryKey);
		}

		public ILazySessionScope ScopeFor<TFactoryKey>()
		{
			return _lazySessions.ScopeFor<TFactoryKey>();
		}

		public virtual ISession CurrentFor(string factoryKey)
		{
			return _lazySessions.CurrentFor(factoryKey);
		}

		public virtual ISession CurrentFor<TFactoryKey>()
		{
			return _lazySessions.CurrentFor<TFactoryKey>();
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
			if (disposing)
				_lazySessions.UnbindAndDisposeCurrent();
			_disposed = true;
		}
	}
}