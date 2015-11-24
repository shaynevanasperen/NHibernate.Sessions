using System;
using NHibernate.Engine;

namespace NHibernate.Sessions
{
	public class LazySession : ILazySession, ILazySessionScoper
	{
		readonly ILazySessionFactory _lazySessionFactory;
		readonly Action<ISession> _onSessionOpened;

		public LazySession(ILazySessionFactory lazySessionFactory, Action<ISession> onSessionOpened = null)
		{
			if (lazySessionFactory == null) throw new ArgumentNullException("lazySessionFactory");

			_lazySessionFactory = lazySessionFactory;
			_onSessionOpened = onSessionOpened;
		}

		public virtual ISession Current
		{
			get { return _lazySessionFactory.Value.GetCurrentOrNewSession(_lazySessionFactory.CurrentSessionContextBinder, _onSessionOpened); }
		}

		public virtual ILazySessionScope Scope()
		{
			return new LazySessionScope(this);
		}

		public virtual void UnbindAndDisposeCurrent()
		{
			if (_lazySessionFactory.IsValueCreated &&
				_lazySessionFactory.CurrentSessionContextBinder.HasBind((ISessionFactoryImplementor)_lazySessionFactory.Value))
				_lazySessionFactory.CurrentSessionContextBinder.Unbind((ISessionFactoryImplementor)_lazySessionFactory.Value).Dispose();
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
				UnbindAndDisposeCurrent();
				_lazySessionFactory.Dispose();
			}
			_disposed = true;
		}
	}

	public class LazySessionScope : ILazySessionScope
	{
		readonly LazySession _lazySession;

		public LazySessionScope(LazySession lazySession)
		{
			_lazySession = lazySession;
		}

		public virtual ISession Current
		{
			get { return _lazySession.Current; }
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
				_lazySession.UnbindAndDisposeCurrent();
			_disposed = true;
		}
	}
}