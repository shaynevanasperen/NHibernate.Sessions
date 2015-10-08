namespace NHibernate.Sessions
{
	public interface IAmbientSessionScope
	{
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
	}
}