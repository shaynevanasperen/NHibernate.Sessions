using System;

namespace NHibernate.Sessions
{
	public interface ISessionManager : IDisposable
	{
		void RegisterSessionFactory(ISessionFactoryEntry sessionFactoryEntry);

		/// <summary>
		/// Gets the current <see cref="ISession"/> if you've configured a single session factory.
		/// If you've configured multiple session factories, invoke <see cref="SessionFor" /> instead.
		/// </summary>
		/// <returns>An <see cref="ISession"/> instance</returns>
		ISession Session { get; }

		/// <summary>
		/// Gets the current <see cref="ISession"/> for the specified <see cref="ISessionFactory"/>.
		/// If you only have one session factory, you should call <see cref="Session"/> instead,
		/// although you're certainly welcome to call this if you have the factory key available.
		/// </summary>
		/// <param name="factoryKey">The factory key of the data context for which you want to retrieve a session</param>
		/// <returns>An <see cref="ISession"/> instance</returns>
		ISession SessionFor(string factoryKey);
		ISession SessionFor<T>();

		/// <summary>
		/// Gets the current <see cref="ISessionFactory"/> if you've configured a single session factory.
		/// If you've configured multiple session factories, invoke <see cref="SessionFactoryFor" /> instead.
		/// </summary>
		/// <returns>An <see cref="ISessionFactory"/> instance</returns>
		ISessionFactory SessionFactory { get; }

		/// <summary>
		/// Gets the current <see cref="ISessionFactory"/> for the specified key.
		/// If you only have one session factory, you should call <see cref="SessionFactory"/> instead,
		/// although you're certainly welcome to call this if you have the factory key available.
		/// </summary>
		/// <param name="factoryKey">The key representing a unique session factory</param>
		/// <returns>An <see cref="ISessionFactory"/> instance</returns>
		ISessionFactory SessionFactoryFor(string factoryKey);
		ISessionFactory SessionFactoryFor<T>();
		
		bool HasMultipleSessionFactories { get; }
	}
}
