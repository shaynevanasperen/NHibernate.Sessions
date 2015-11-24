using System;

namespace NHibernate.Sessions
{
	public interface ILazySessionFactories : IDisposable
	{
		/// <summary>
		/// Gets the <see cref="ISessionFactory"/> associated with the specified key.
		/// </summary>
		/// <param name="key">The key associated with the session factory</param>
		/// <returns>An <see cref="ISessionFactory"/> instance</returns>
		ISessionFactory ValueFor(string key);

		/// <summary>
		/// Gets the <see cref="ISessionFactory"/> associated with a key matching the specified type's name.
		/// </summary>
		/// <typeparam name="TFactoryKey">A type whose name matches a key asscociated with an <see cref="ISessionFactory"/></typeparam>
		/// <returns>An <see cref="ISessionFactory"/> instance</returns>
		ISessionFactory ValueFor<TFactoryKey>();
	}
}
