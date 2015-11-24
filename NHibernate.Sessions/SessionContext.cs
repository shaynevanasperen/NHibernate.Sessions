using System;

namespace NHibernate.Sessions
{
	public abstract class SessionContext : ISessionContext, IDisposable
	{
		readonly Lazy<ISession> _session;
		readonly ILazySessionScope _lazySessionScope;

		protected SessionContext(Func<ISession> sessionFunc)
		{
			_session = new Lazy<ISession>(sessionFunc);
		}

		protected SessionContext(ILazySessionScope lazySessionScope)
		{
			_lazySessionScope = lazySessionScope;
		}

		public virtual ISession Session
		{
			get { return _session != null ? _session.Value : _lazySessionScope.Current; }
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
				if (_session != null && _session.IsValueCreated)
					_session.Value.Dispose();
				else if (_lazySessionScope != null)
					_lazySessionScope.Dispose();
			}
			_disposed = true;
		}
	}
}
