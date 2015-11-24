using System;
using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Sessions
{
	public class LazySessionFactories : ILazySessionFactories
	{
		readonly IDictionary<string, ILazySessionFactory> _lazySessionFactories;

		public LazySessionFactories(IDictionary<string, ILazySessionFactory> lazySessionFactories)
		{
			if (lazySessionFactories == null) throw new ArgumentNullException("lazySessionFactories");
			if (!lazySessionFactories.Any()) throw new ArgumentException("lazySessionFactories cannot be empty.", "lazySessionFactories");

			_lazySessionFactories = lazySessionFactories;
		}

		public virtual ISessionFactory ValueFor(string key)
		{
			return _lazySessionFactories[key].Value;
		}

		public virtual ISessionFactory ValueFor<TFactoryKey>()
		{
			return ValueFor(typeof(TFactoryKey).Name);
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
				foreach (var lazySessionFactory in _lazySessionFactories.Values)
					lazySessionFactory.Dispose();
				_lazySessionFactories.Clear();
			}
			_disposed = true;
		}
	}
}