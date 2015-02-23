namespace NHibernate.Sessions
{
	public interface ISessionFactoryEntry
	{
		string Key { get; }
		SessionFactoryInitializationMode InitializationMode { get; }
		ISessionFactory SessionFactory { get; }
		bool IsInitialized { get; }
		ISession Session { get; }
	}
}
