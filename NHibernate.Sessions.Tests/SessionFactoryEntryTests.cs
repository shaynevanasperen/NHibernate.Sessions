using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Context;
using NHibernate.Engine;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_for_lazy_initialization : WithFakes
	{
		It should_defer_initialization_of_the_session_factory_until_first_use = () =>
			sessionFactoryInitializationWasDeferred.ShouldBeTrue();

		It should_not_allow_the_initialization_to_run_more_than_once = () =>
		{
			Parallel.Invoke(Enumerable.Repeat<System.Action>(() => initializedSessionFactory = sessionFactoryEntry.SessionFactory, 10).ToArray());
			sessionFactoryInitializationCount.ShouldEqual(1);
		};

		Because of = () =>
			initializedSessionFactory = sessionFactoryEntry.SessionFactory;

		Establish context = () =>
			sessionFactoryEntry = new SessionFactoryEntry("Key", getSessionFactory, SessionFactoryInitializationMode.Lazy);

		static ISessionFactory getSessionFactory()
		{
			Interlocked.Increment(ref sessionFactoryInitializationCount);
			sessionFactoryInitializationWasDeferred = sessionFactoryEntry != null;
			return An<ISessionFactory>();
		}

		static int sessionFactoryInitializationCount;
		static SessionFactoryEntry sessionFactoryEntry;
		static bool sessionFactoryInitializationWasDeferred;
		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_for_eager_initialization : WithFakes
	{
		It should_initialize_the_session_factory_immediately = () =>
			sessionFactoryInitializationWasDeferred.ShouldBeFalse();

		Because of = () =>
			initializedSessionFactory = sessionFactoryEntry.SessionFactory;

		Establish context = () =>
			sessionFactoryEntry = new SessionFactoryEntry("Key", getSessionFactory, SessionFactoryInitializationMode.Eager);

		static ISessionFactory getSessionFactory()
		{
			sessionFactoryInitializationWasDeferred = sessionFactoryEntry != null;
			return An<ISessionFactory>();
		}

		static SessionFactoryEntry sessionFactoryEntry;
		static bool sessionFactoryInitializationWasDeferred;
		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_for_threaded_initialization : WithFakes
	{
		It should_initialize_the_session_factory_in_another_thread = () =>
			sessionFactoryInitializationThreadId.ShouldNotEqual(Thread.CurrentThread.ManagedThreadId);

		It should_not_allow_the_initialization_to_run_more_than_once = () =>
		{
			Parallel.Invoke(Enumerable.Repeat<System.Action>(() => initializedSessionFactory = sessionFactoryEntry.SessionFactory, 10).ToArray());
			sessionFactoryInitializationCount.ShouldEqual(1);
		};

		Because of = () =>
		{
			sessionFactoryEntry = new SessionFactoryEntry("Key", getSessionFactory, SessionFactoryInitializationMode.Threaded);
			waitHandle.WaitOne();
		};

		static ISessionFactory getSessionFactory()
		{
			sessionFactoryInitializationThreadId = Thread.CurrentThread.ManagedThreadId;
			waitHandle.Set();
			Interlocked.Increment(ref sessionFactoryInitializationCount);
			return initializedSessionFactory = An<ISessionFactory>();
		}

		static int sessionFactoryInitializationCount;
		static int sessionFactoryInitializationThreadId;
		static EventWaitHandle waitHandle = new AutoResetEvent(false);
		static SessionFactoryEntry sessionFactoryEntry;
		static ISessionFactory initializedSessionFactory;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_for_threaded_initialization_without_an_on_session_factory_threaded_initialization_exception_action_and_an_exception_is_thrown_during_initialization : WithFakes
	{
		It should_result_in_an_unhandled_exception = () =>
			Catch.Exception(() => sessionFactoryEntry.SessionFactoryInitializationTask.Wait()).ShouldNotBeNull();

		Establish context = () =>
			sessionFactoryEntry = new SessionFactoryEntry("Key", getSessionFactory, SessionFactoryInitializationMode.Threaded);

		static ISessionFactory getSessionFactory()
		{
			throw new Exception();
		}

		static SessionFactoryEntry sessionFactoryEntry;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_for_threaded_initialization_with_an_on_session_factory_threaded_initialization_exception_action_and_an_exception_is_thrown_during_initialization : WithFakes
	{
		It should_not_result_in_an_unhandled_exception = () =>
			Catch.Exception(() => sessionFactoryEntry.SessionFactoryInitializationTask.Wait()).ShouldBeNull();

		It should_invoke_the_on_session_factory_threaded_initialization_action = () =>
			sessionFactoryThreadedInitializationException.ShouldNotBeNull();

		Establish context = () =>
			sessionFactoryEntry = new SessionFactoryEntry("Key", getSessionFactory, SessionFactoryInitializationMode.Threaded,
				null, null, onSessionFactoryThreadedInitializationException);

		static ISessionFactory getSessionFactory()
		{
			throw new Exception();
		}

		static void onSessionFactoryThreadedInitializationException(Exception exception)
		{
			sessionFactoryThreadedInitializationException = exception;
		}

		static SessionFactoryEntry sessionFactoryEntry;
		static Exception sessionFactoryThreadedInitializationException;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_with_an_on_session_factory_initialized_action_and_an_instance_of_session_factory : WithFakes
	{
		It should_invoke_the_on_session_factory_initialized_action = () =>
			onSessionFactoryInitializedInvoked.ShouldBeTrue();

		Establish context = () =>
			new SessionFactoryEntry("Key", An<ISessionFactory>(), onSessionFactoryInitialized);

		static void onSessionFactoryInitialized(ISessionFactory sessionFactory)
		{
			onSessionFactoryInitializedInvoked = true;
		}

		static bool onSessionFactoryInitializedInvoked;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_with_an_on_session_factory_initialized_action_and_a_session_factory_provider : WithFakes
	{
		It should_invoke_the_on_session_factory_initialized_action = () =>
			onSessionFactoryInitializedInvoked.ShouldBeTrue();

		Establish context = () =>
			new SessionFactoryEntry("Key", getSessionFactory, SessionFactoryInitializationMode.Eager, onSessionFactoryInitialized);

		static ISessionFactory getSessionFactory()
		{
			return An<ISessionFactory>();
		}

		static void onSessionFactoryInitialized(ISessionFactory sessionFactory)
		{
			onSessionFactoryInitializedInvoked = true;
		}

		static bool onSessionFactoryInitializedInvoked;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_without_a_current_session_context_and_an_on_session_opened_action : WithFakes
	{
		It should_return_a_different_session_upon_subsequent_requests_within_the_same_context = () =>
			firstRequestedSession.ShouldNotBeTheSameAs(secondRequestedSession);

		It should_invoke_the_on_session_opened_action_upon_each_request_of_a_session = () =>
		{
			capturedSession.ShouldBeTheSameAs(secondRequestedSession);
			onSessionOpenedCallCount.ShouldEqual(2);
		};

		Because of = () =>
		{
			firstRequestedSession = sessionFactoryEntry.Session;
			secondRequestedSession = sessionFactoryEntry.Session;
		};

		Establish context = () =>
		{
			var sessionFactory = An<ISessionFactory>();
			sessionFactory.WhenToldTo(x => x.OpenSession()).Return(() => An<ISession>());
			sessionFactoryEntry = new SessionFactoryEntry("Key", sessionFactory, null, onSessionOpened);
			capturedSession.ShouldBeNull();
		};

		static void onSessionOpened(ISession session)
		{
			onSessionOpenedCallCount++;
			capturedSession = session;
		}

		static SessionFactoryEntry sessionFactoryEntry;
		static ISession capturedSession;
		static ISession firstRequestedSession;
		static ISession secondRequestedSession;
		static int onSessionOpenedCallCount;
	}

	[Subject(typeof(SessionFactoryEntry))]
	class When_configured_with_a_current_session_context_and_an_on_session_opened_action : WithFakes
	{
		It should_return_the_same_session_upon_subsequent_requests_within_the_same_context = () =>
			firstRequestedSession.ShouldBeTheSameAs(secondRequestedSession);

		It should_invoke_the_on_session_opened_action_only_the_first_time_a_session_is_requested_within_the_same_context = () =>
		{
			capturedSession.ShouldBeTheSameAs(firstRequestedSession);
			onSessionOpenedCallCount.ShouldEqual(1);
		};

		Because of = () =>
		{
			firstRequestedSession = sessionFactoryEntry.Session;
			secondRequestedSession = sessionFactoryEntry.Session;
		};

		Establish context = () =>
		{
			var sessionFactory = An<ISessionFactoryImplementor>();
			var currentSessionContext = new ThreadStaticSessionContext(sessionFactory);
			sessionFactory.WhenToldTo(x => x.CurrentSessionContext).Return(currentSessionContext);
			sessionFactory.WhenToldTo(x => x.GetCurrentSession()).Return(currentSessionContext.CurrentSession);
			sessionFactory.WhenToldTo(x => x.OpenSession()).Return(() =>
			{
				var session = An<ISession>();
				session.WhenToldTo(x => x.SessionFactory).Return(sessionFactory);
				return session;
			});
			sessionFactoryEntry = new SessionFactoryEntry("Key", sessionFactory, null, onSessionOpened);
			capturedSession.ShouldBeNull();
		};

		static void onSessionOpened(ISession session)
		{
			onSessionOpenedCallCount++;
			capturedSession = session;
		}

		static SessionFactoryEntry sessionFactoryEntry;
		static ISession capturedSession;
		static ISession firstRequestedSession;
		static ISession secondRequestedSession;
		static int onSessionOpenedCallCount;
	}
}
