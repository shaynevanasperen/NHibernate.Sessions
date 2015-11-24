namespace NHibernate.Sessions
{
	public interface ICurrentSessionContextBinder
	{
		bool HasBind(ISessionFactory sessionFactory);
		ISession BindNew(ISessionFactory sessionFactory);
		ISession Unbind(ISessionFactory sessionFactory);
	}
}