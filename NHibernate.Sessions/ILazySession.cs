using System;

namespace NHibernate.Sessions
{
	public interface ILazySession
	{
		/// <summary>
		/// Gets the current <see cref="ISession"/>, as defined by the underlying <see cref="ISessionFactory"/>'s configured current session context.
		/// </summary>
		ISession Current { get; }
	}

	public interface ILazySessionScope : ILazySession, IDisposable { }

	public interface ILazySessionScoper : IDisposable
	{
		ILazySessionScope Scope();
		void UnbindAndDisposeCurrent();
	}
}