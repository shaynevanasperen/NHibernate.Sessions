using System;
using NHibernate.Context;

namespace NHibernate.Sessions
{
	/// <summary>
	/// Warning: This won't work if you're using <see cref="ThreadStaticSessionContext"/> as your current session context class.
	/// </summary>
	public interface ILazySessions
	{
		/// <summary>
		/// Gets the current <see cref="ISession"/> for the <see cref="ISessionFactory"/> associated with the specified key.
		/// </summary>
		/// <param name="factoryKey">The key associated with the session factory for which you want to retrieve the current session</param>
		/// <returns>An <see cref="ISession"/> instance</returns>
		ISession CurrentFor(string factoryKey);

		/// <summary>
		/// Gets the current <see cref="ISession"/> for the <see cref="ISessionFactory"/> associated with a key matching the specified type's name.
		/// </summary>
		/// <typeparam name="TFactoryKey">A type whose name matches a key asscociated with an <see cref="ISessionFactory"/></typeparam>
		/// <returns>An <see cref="ISession"/> instance</returns>
		ISession CurrentFor<TFactoryKey>();
	}

	public interface ILazySessionsScope : ILazySessions, IDisposable { }

	public interface ILazySessionsScoper : IDisposable
	{
		ILazySessionScope ScopeFor(string factoryKey);
		ILazySessionScope ScopeFor<TFactoryKey>();
		ILazySessionsScope Scope();
		void UnbindAndDisposeCurrent();
	}
}