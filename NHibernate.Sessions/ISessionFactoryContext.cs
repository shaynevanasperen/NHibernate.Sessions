namespace NHibernate.Sessions
{
	public interface ISessionFactoryContext
	{
		ISessionFactory SessionFactory { get; }
	}
}