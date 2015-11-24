namespace NHibernate.Sessions
{
	public interface ISessionContext
	{
		ISession Session { get; }
	}
}