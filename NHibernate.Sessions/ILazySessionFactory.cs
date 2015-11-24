using System;

namespace NHibernate.Sessions
{
	public interface ILazySessionFactory : IDisposable
	{
		ICurrentSessionContextBinder CurrentSessionContextBinder { get; }
		bool IsValueCreated { get; }
		ISessionFactory Value { get; }
	}
}