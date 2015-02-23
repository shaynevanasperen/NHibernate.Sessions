namespace NHibernate.Sessions.Configuration
{
	public class ConfigurationCacheNegatedSetting : IConfigurationCacheNegatedSetting
	{
		readonly Configure _parent;
		readonly SessionFactoryRegistration _sessionFactoryRegistration;

		internal ConfigurationCacheNegatedSetting(Configure parent, SessionFactoryRegistration sessionFactoryRegistration)
		{
			_parent = parent;
			_sessionFactoryRegistration = sessionFactoryRegistration;
		}

		public IPartialConfiguration Cached()
		{
			_sessionFactoryRegistration.ConfigurationCache = null;
			return _parent;
		}
	}
}
