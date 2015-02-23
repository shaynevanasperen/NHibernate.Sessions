using System;

namespace NHibernate.Sessions.Configuration
{
	public interface IBeginConfiguration
	{
		IPartialConfiguration Using(Func<string, Cfg.Configuration> configuration);
	}
}