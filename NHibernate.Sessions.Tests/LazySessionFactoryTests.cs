using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Sessions.Configuration;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(LazySessionFactory))]
	class When_configured_for_lazy_initialization : WithSubject<LazySessionFactory>
	{
		It should_report_that_value_is_not_created_before_first_use = () =>
			isValueCreatedBeforeFirstUse.ShouldBeFalse();

		It should_defer_initialization_of_the_session_factory_until_first_use = () =>
			initializationWasDeferred.ShouldBeTrue();

		It should_not_allow_the_initialization_to_run_more_than_once = () =>
			initializationCount.ShouldEqual(1);

		Because of = () =>
		{
			isValueCreatedBeforeFirstUse = Subject.IsValueCreated;
			initializationWasDeferred = !configurationProvided;
			Parallel.Invoke(Enumerable.Repeat<System.Action>(() => initializedSessionFactory = Subject.Value, 10).ToArray());
		};

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			Interlocked.Increment(ref initializationCount);
			configurationProvided = true;
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration();
		}

		static int initializationCount;
		static bool configurationProvided;
		static bool isValueCreatedBeforeFirstUse;
		static bool initializationWasDeferred;
		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_configured_for_eager_initialization : WithSubject<LazySessionFactory>
	{
		It should_report_that_value_is_created_before_first_use = () =>
			isValueCreatedBeforeFirstUse.ShouldBeTrue();

		It should_initialize_the_session_factory_immediately = () =>
			initializationWasDeferred.ShouldBeFalse();

		It should_not_allow_the_initialization_to_run_more_than_once = () =>
			initializationCount.ShouldEqual(1);

		Because of = () =>
		{
			isValueCreatedBeforeFirstUse = Subject.IsValueCreated;
			initializationWasDeferred = !configurationProvided;
			Parallel.Invoke(Enumerable.Repeat<System.Action>(() => initializedSessionFactory = Subject.Value, 10).ToArray());
		};

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Eager);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			Interlocked.Increment(ref initializationCount);
			configurationProvided = true;
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration();
		}

		static int initializationCount;
		static bool configurationProvided;
		static bool isValueCreatedBeforeFirstUse;
		static bool initializationWasDeferred;
		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_configured_for_background_initialization : WithSubject<LazySessionFactory>
	{
		It should_initialize_the_session_factory_in_a_background_thread = () =>
		{
			initializationThreadId.ShouldNotEqual(Thread.CurrentThread.ManagedThreadId);
			initializationThreadIsBackground.ShouldBeTrue();
		};

		It should_not_allow_the_initialization_to_run_more_than_once = () =>
		{
			Parallel.Invoke(Enumerable.Repeat<System.Action>(() => initializedSessionFactory = Subject.Value, 10).ToArray());
			initializationCount.ShouldEqual(1);
		};

		Because of = () =>
			Subject.ValueCreationTask.Wait();

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Background);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			initializationThreadId = Thread.CurrentThread.ManagedThreadId;
			initializationThreadIsBackground = Thread.CurrentThread.IsBackground;
			Interlocked.Increment(ref initializationCount);
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration();
		}

		static int initializationCount;
		static int initializationThreadId;
		static bool initializationThreadIsBackground;
		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_configured_for_background_initialization_without_an_on_background_initialization_exception_action_and_an_exception_is_thrown_during_initialization : WithSubject<LazySessionFactory>
	{
		It should_result_in_an_unhandled_exception = () =>
			Catch.Exception(() => Subject.ValueCreationTask.Wait()).ShouldNotBeNull();

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Background);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			throw new Exception();
		}
	}

	[Subject(typeof(LazySessionFactory))]
	class When_configured_for_background_initialization_with_an_on_background_initialization_exception_action_and_an_exception_is_thrown_during_initialization : WithSubject<LazySessionFactory>
	{
		It should_not_result_in_an_unhandled_exception = () =>
			Catch.Exception(() => Subject.ValueCreationTask.Wait()).ShouldBeNull();

		It should_invoke_the_on_session_factory_threaded_initialization_action = () =>
			sessionFactoryThreadedInitializationException.ShouldNotBeNull();

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Background);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.OnBackgroundInitializationException).Return(onSessionFactoryThreadedInitializationException);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			throw new Exception();
		}

		static void onSessionFactoryThreadedInitializationException(Exception exception)
		{
			sessionFactoryThreadedInitializationException = exception;
		}

		static Exception sessionFactoryThreadedInitializationException;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_configured_with_an_on_initialized_action : WithSubject<LazySessionFactory>
	{
		It should_invoke_the_on_initialized_action = () =>
			initializedSessionFactory.ShouldEqual(Subject.Value);

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Eager);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.OnInitialized).Return(onInitialized);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration();
		}

		static void onInitialized(ISessionFactory sessionFactory)
		{
			initializedSessionFactory = sessionFactory;
		}

		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_not_specified_a_current_session_context_binder_class_and_a_current_session_context_is_configured_but_the_initialized_session_factory_does_not_implement_isessionfactoryimplementor : WithSubject<TestLazySessionFactory>
	{
		It should_throw_a_hibernate_exception_stating_that_the_session_factory_does_not_implement_isessionfactoryimplementor = () =>
		{
			exception.ShouldBeOfExactType<HibernateException>();
			exception.Message.ShouldEqual("Session factory does not implement ISessionFactoryImplementor.");
		};

		Because of = () =>
			exception = Catch.Exception(() => Subject.CurrentSessionContextBinder);

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration();
		}

		static Exception exception;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_not_specified_a_current_session_context_binder_class_and_no_current_session_context_is_configured : WithSubject<LazySessionFactory>
	{
		It should_throw_a_hibernate_exception_stating_that_no_current_session_context_is_configured = () =>
		{
			exception.ShouldBeOfExactType<HibernateException>();
			exception.Message.ShouldEqual("No current session context configured.");
		};

		Because of = () =>
			exception = Catch.Exception(() => Subject.CurrentSessionContextBinder);

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration();
		}

		static Exception exception;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_not_specified_a_current_session_context_binder_class_and_a_current_session_context_is_configured : WithSubject<LazySessionFactory>
	{
		It should_return_the_default_current_session_context_binder_compatible_with_current_session_context = () =>
			currentSessionContextBinder.ShouldBeTheSameAs(CurrentSessionContextBinder.Instance);

		Because of = () =>
			currentSessionContextBinder = Subject.CurrentSessionContextBinder;

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).CurrentSessionContext<ThreadStaticSessionContext>().BuildConfiguration();
		}

		static ICurrentSessionContextBinder currentSessionContextBinder;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_not_specified_a_current_session_context_binder_class_and_a_thread_local_session_context_is_configured : WithSubject<LazySessionFactory>
	{
		It should_return_the_default_current_session_context_binder_compatible_with_thread_local_session_context = () =>
			currentSessionContextBinder.ShouldBeTheSameAs(ThreadLocalSessionContextBinder.Instance);

		Because of = () =>
			currentSessionContextBinder = Subject.CurrentSessionContextBinder;

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).CurrentSessionContext<ThreadLocalSessionContext>().BuildConfiguration();
		}

		static ICurrentSessionContextBinder currentSessionContextBinder;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_not_specified_a_current_session_context_binder_class_and_a_custom_current_session_context_incompatible_with_current_session_context_and_thread_local_session_context_is_configured : WithSubject<LazySessionFactory>
	{
		It should_throw_a_nibernate_exception_stating_that_a_default_current_session_context_binder_could_not_be_provided_as_the_configured_current_session_context_is_not_compatible_with_either_current_session_context_or_thread_local_session_context = () =>
		{
			exception.ShouldBeOfExactType<HibernateException>();
			exception.Message.ShouldEqual("Could not provide a default ICurrentSessionContextBinder " +
										  "as the configured current session context is not compatible " +
										  "with either CurrentSessionContext or ThreadLocalSessionContext.");
		};

		Because of = () =>
			exception = Catch.Exception(() => Subject.CurrentSessionContextBinder);

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration().CurrentSessionContext<CustomCurrentSessionContext>();
		}

		static Exception exception;

		class CustomCurrentSessionContext : ICurrentSessionContext
		{
			public CustomCurrentSessionContext(ISessionFactory sessionFactory) { }

			public ISession CurrentSession()
			{
				throw new NotSupportedException();
			}
		}
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_specified_a_current_session_context_binder_class_which_is_not_valid : WithSubject<LazySessionFactory>
	{
		It should_throw_a_type_load_exception = () =>
			exception.ShouldBeOfExactType<TypeLoadException>();

		Because of = () =>
			exception = Catch.Exception(() => Subject.CurrentSessionContextBinder);

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration().CurrentSessionContextBinder("invalid");
		}

		static Exception exception;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_specified_a_current_session_context_binder_class_which_does_not_implement_icurrentsessioncontextbinder : WithSubject<LazySessionFactory>
	{
		It should_throw_an_exception_stating_that_the_specified_type_does_not_implement_icurrentsessioncontextbinder = () =>
			exception.Message.ShouldEqual(string.Format("The type specified by the '{0}' property does not implement ICurrentSessionContextBinder.",
				ConfigurationExtension.CurrentSessionContextBinderClass));

		Because of = () =>
			exception = Catch.Exception(() => Subject.CurrentSessionContextBinder);

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration().CurrentSessionContextBinder(typeof(CustomCurrentSessionContextBinder).AssemblyQualifiedName);
		}

		static Exception exception;

		class CustomCurrentSessionContextBinder { }
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_session_context_binder_and_the_configuration_has_specified_a_current_session_context_binder_class_which_implements_icurrentsessioncontextbinder : WithSubject<LazySessionFactory>
	{
		It should_return_an_instance_of_the_specified_current_session_context_binder = () =>
			currentSessionContextBinder.ShouldBeOfExactType<CustomCurrentSessionContextBinder>();

		Because of = () =>
			currentSessionContextBinder = Subject.CurrentSessionContextBinder;

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return((Func<Cfg.Configuration>)getConfiguration);
		};

		static Cfg.Configuration getConfiguration()
		{
			return Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory()).BuildConfiguration().CurrentSessionContextBinder<CustomCurrentSessionContextBinder>();
		}

		static ICurrentSessionContextBinder currentSessionContextBinder;

		class CustomCurrentSessionContextBinder : ICurrentSessionContextBinder
		{
			public bool HasBind(ISessionFactory sessionFactory)
			{
				throw new NotSupportedException();
			}

			public ISession BindNew(ISessionFactory sessionFactory)
			{
				throw new NotSupportedException();
			}

			public ISession Unbind(ISessionFactory sessionFactory)
			{
				throw new NotSupportedException();
			}
		}
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_before_initialized : WithSubject<LazySessionFactory>
	{
		It should_not_initialize = () =>
			Subject.IsValueCreated.ShouldBeFalse();

		Because of = () =>
			Subject.Dispose();

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return(() => (Cfg.Configuration)null);
		};
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_after_initialized : WithSubject<TestLazySessionFactory>
	{
		It should_dispose_the_underlying_session_factory_only_once = () =>
			Subject.Value.WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
		{
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.InitializationMode).Return(SessionFactoryInitializationMode.Lazy);
			The<ISessionFactoryInitializion>().WhenToldTo(x => x.ConfigurationProvider).Return(() => (Cfg.Configuration)null);
		};
	}

	internal class TestLazySessionFactory : LazySessionFactory
	{
		readonly ISessionFactory _sessionFactory;

		public TestLazySessionFactory(ISessionFactory sessionFactory, ISessionFactoryInitializion sessionFactoryInitializion)
			: base(sessionFactoryInitializion)
		{
			_sessionFactory = sessionFactory;
		}

		public override ISessionFactory Value
		{
			get { return _sessionFactory; }
		}

		public override bool IsValueCreated
		{
			get { return true; }
		}
	}
}
