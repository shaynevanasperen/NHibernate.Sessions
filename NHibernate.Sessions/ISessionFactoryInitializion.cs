using System;

namespace NHibernate.Sessions
{
	public interface ISessionFactoryInitializion
	{
		Func<Cfg.Configuration> ConfigurationProvider { get; }
		SessionFactoryInitializationMode InitializationMode { get; }
		Action<Exception> OnBackgroundInitializationException { get; }
		Action<ISessionFactory> OnInitialized { get; }
	}
}