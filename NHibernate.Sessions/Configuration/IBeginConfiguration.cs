using System;

namespace NHibernate.Sessions.Configuration
{
	public interface IBeginConfiguration
	{
		IPartialConfiguration UsingConfigurationFactory(Func<string, Cfg.Configuration> configuration);
	}
}