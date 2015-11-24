using System;

namespace NHibernate.Sessions
{
	public abstract class SessionFactoryContext : ISessionFactoryContext, IDisposable
	{
		readonly Lazy<ISessionFactory> _sessionFactory;
		readonly ILazySessionFactory _lazySessionFactory;

		protected SessionFactoryContext(Func<ISessionFactory> sessionFactoryFunc)
		{
			_sessionFactory = new Lazy<ISessionFactory>(sessionFactoryFunc);
		}

		protected SessionFactoryContext(ILazySessionFactory lazySessionFactory)
		{
			_lazySessionFactory = lazySessionFactory;
		}

		public virtual ISessionFactory SessionFactory
		{
			get { return _sessionFactory != null ? _sessionFactory.Value : _lazySessionFactory.Value; }
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
				if (_sessionFactory != null && _sessionFactory.IsValueCreated)
					_sessionFactory.Value.Dispose();
				else if (_lazySessionFactory != null)
					_lazySessionFactory.Dispose();
			}
			_disposed = true;
		}
	}
}
