using System;
using Machine.Fakes;
using Machine.Specifications;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(SessionFactoryContext))]
	class When_getting_session_factory_from_a_session_factory_context_constructed_with_a_session_factory_func : WithSubject<SessionFactoryFuncContext>
	{
		It should_only_call_the_func_once_and_then_cache_it_for_subsequent_calls = () =>
			funcExecuteCount.ShouldEqual(1);

		Because of = () =>
		{
			sessionFactory = Subject.SessionFactory;
			sessionFactory = Subject.SessionFactory;
		};

		Establish context = () =>
			Configure<Func<ISessionFactory>>(getSessionFactory);

		static ISessionFactory getSessionFactory()
		{
			funcExecuteCount++;
			return An<ISessionFactory>();
		}

		static int funcExecuteCount;
		static ISessionFactory sessionFactory;
	}

	[Subject(typeof(SessionFactoryContext))]
	class When_disposing_a_session_factory_context_constructed_with_a_session_factory_func_and_session_factory_has_not_been_accessed : WithSubject<SessionFactoryFuncContext>
	{
		It should_not_call_the_func = () =>
			funcExecuteCount.ShouldEqual(0);

		Because of = () =>
			Subject.Dispose();

		Establish context = () =>
			Configure<Func<ISessionFactory>>(getSessionFactory);

		static ISessionFactory getSessionFactory()
		{
			funcExecuteCount++;
			return An<ISessionFactory>();
		}

		static int funcExecuteCount;
	}

	[Subject(typeof(SessionFactoryContext))]
	class When_disposing_a_session_factory_context_constructed_with_a_session_factory_func_and_session_factory_has_been_accessed : WithSubject<SessionFactoryFuncContext>
	{
		It should_dispose_the_session_factory_obtained_from_the_func_only_once = () =>
			sessionFactory.WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			sessionFactory = Subject.SessionFactory;
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
			Configure<Func<ISessionFactory>>(getSessionFactory);

		static ISessionFactory getSessionFactory()
		{
			funcExecuteCount++;
			return An<ISessionFactory>();
		}

		static int funcExecuteCount;
		static ISessionFactory sessionFactory;
	}

	class SessionFactoryFuncContext : SessionFactoryContext
	{
		public SessionFactoryFuncContext(Func<ISessionFactory> sessionFactoryFunc) : base(sessionFactoryFunc) { }
	}

	[Subject(typeof(SessionFactoryContext))]
	class When_getting_session_factory_from_a_session_factory_context_constructed_with_a_lazy_session_factory : WithSubject<LazySessionFactoryContext>
	{
		It should_return_the_session_factory = () =>
			sessionFactory.ShouldBeTheSameAs(The<ILazySessionFactory>().Value);

		Because of = () =>
			sessionFactory = Subject.SessionFactory;

		Establish context = () =>
			The<ILazySessionScope>().WhenToldTo(x => x.Current).Return(The<ISession>());

		static ISessionFactory sessionFactory;
	}

	[Subject(typeof(SessionFactoryContext))]
	class When_disposing_a_session_factory_context_constructed_with_a_lazy_session_factory : WithSubject<LazySessionFactoryContext>
	{
		It should_dispose_the_lazy_session_scope_only_once = () =>
			The<ILazySessionFactory>().WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactory>());
	}

	class LazySessionFactoryContext : SessionFactoryContext
	{
		public LazySessionFactoryContext(ILazySessionFactory lazySessionFactory) : base(lazySessionFactory) { }
	}
}
