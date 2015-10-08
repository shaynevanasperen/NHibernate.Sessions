using System;
using System.Reflection;

namespace NHibernate.Sessions.Configuration
{
	public interface IPartialConfiguration : IBeginConfiguration
	{
		IConfigurationCacheSetting WithConfiguration { get; }
		IInitializationSetting WithInitialization { get; }
		ISessionFactorySetting WithSessionFactory { get; }
		ISessionSetting WithSession { get; }

		ICompleteConfiguration RegisterSessionFactory(string sessionFactoryKey);
	}

	public interface IConfigurationCacheSetting
	{
		IPartialConfiguration Cached();
		IPartialConfiguration Cached(params string[] dependentFilePaths);
		IPartialConfiguration Cached(IConfigurationCache configurationCache, params string[] dependentFilePaths);
		IPartialConfiguration Cached(params Assembly[] dependentAssemblies);
		IPartialConfiguration Cached(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies);
		IConfigurationCacheNegatedSetting Not { get; }
	}

	public interface IConfigurationCacheNegatedSetting
	{
		IPartialConfiguration Cached();
	}

	public interface IInitializationSetting
	{
		IPartialConfiguration Eager();
		IPartialConfiguration Lazy();
		IPartialConfiguration Threaded(Action<Exception> onSessionFactoryThreadedInitializationException = null);
	}

	public interface ISessionFactorySetting
	{
		IPartialConfiguration OnInitialized(Action<ISessionFactory> onSessionFactoryInitialized);
	}

	public interface ISessionSetting
	{
		IPartialConfiguration OnOpened(Action<ISession> onSessionOpened);
	}
}