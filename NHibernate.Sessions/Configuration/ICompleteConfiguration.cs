namespace NHibernate.Sessions.Configuration
{
	public interface ICompleteConfiguration : IPartialConfiguration
	{
		ISessionManager BuildSessionManager();
	}
}