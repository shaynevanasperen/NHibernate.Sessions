using System;
using System.Threading;

namespace NHibernate.Sessions
{
	public interface ISessionFactoryInitializion
	{
		Func<Cfg.Configuration> ConfigurationProvider { get; }
		SessionFactoryInitializationMode InitializationMode { get; }
		Action<Exception> OnBackgroundInitializationException { get; }
		Action<Thread> OnBackgroundInitializationStarted { get; }
		Action<Thread> OnBackgroundInitializationCompleted { get; }
		Action<ISessionFactory> OnInitialized { get; }
	}
}