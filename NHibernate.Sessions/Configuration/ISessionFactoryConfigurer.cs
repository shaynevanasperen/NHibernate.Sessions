using System;
using System.Reflection;
using System.Threading;
using NHibernate.Context;

namespace NHibernate.Sessions.Configuration
{
	public interface ISessionFactoryConfigurer<out T> where T : ISessionFactoryConfigurer<T>
	{
		T WithoutCachedConfiguration();
		T WithCachedConfiguration();
		T WithCachedConfiguration(params string[] dependentFilePaths);
		T WithCachedConfiguration(IConfigurationCache configurationCache, params string[] dependentFilePaths);
		T WithCachedConfiguration(params Assembly[] dependentAssemblies);
		T WithCachedConfiguration(IConfigurationCache configurationCache, params Assembly[] dependentAssemblies);

		T WithEagerInitialization();
		T WithLazyInitialization();
		T WithBackgroundInitialization(Action<Exception> onBackgroundInitializationException = null,
			Action<Thread> onBackgroundInitializationStarted = null, Action<Thread> onBackgroundInitializationCompleted = null);

		T WithInitializedCallback(Action<ISessionFactory> onInitialized);
		T WithSessionOpenedCallback(Action<ISession> onSessionOpened);
	}

	public interface ISingleSessionFactoryConfigurer : ISessionFactoryConfigurer<ISingleSessionFactoryConfigurer>
	{
		LazySessionFactory CreateLazySessionFactory();
		LazySession CreateLazySession();
	}

	public interface IMultiSessionFactoryConfigurer<out TSessionFactoryConfigurer, out TMultiSessionFactoryConfigurer> : ISessionFactoryConfigurer<TSessionFactoryConfigurer>
		where TSessionFactoryConfigurer : ISessionFactoryConfigurer<TSessionFactoryConfigurer>
	{
		TMultiSessionFactoryConfigurer RegisterSessionFactory(string key);
		TMultiSessionFactoryConfigurer RegisterSessionFactory<TFactoryKey>();
	}

	public interface IMultiSessionFactoryConfigurer : IMultiSessionFactoryConfigurer<IMultiSessionFactoryConfigurer, IMultiSessionFactoryConfigurerWithRegistration> { }

	public interface IMultiSessionFactoryConfigurerWithRegistration : IMultiSessionFactoryConfigurer<IMultiSessionFactoryConfigurerWithRegistration, IMultiSessionFactoryConfigurerWithRegistration>
	{
		LazySessionFactories CreateLazySessionFactories();

		/// <summary>
		/// Warning: This won't work if you're using <see cref="ThreadStaticSessionContext"/> as your current session context class.
		/// </summary>
		LazySessions CreateLazySessions();
	}
}