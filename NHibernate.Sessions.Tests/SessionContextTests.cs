using System;
using Machine.Fakes;
using Machine.Specifications;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(SessionContext))]
	class When_getting_session_from_a_session_context_constructed_with_a_session_func : WithSubject<SessionFuncSessionContext>
	{
		It should_only_call_the_func_once_and_then_cache_it_for_subsequent_calls = () =>
			funcExecuteCount.ShouldEqual(1);

		Because of = () =>
		{
			session = Subject.Session;
			session = Subject.Session;
		};

		Establish context = () =>
			Configure<Func<ISession>>(getSession);

		static ISession getSession()
		{
			funcExecuteCount++;
			return An<ISession>();
		}

		static int funcExecuteCount;
		static ISession session;
	}

	[Subject(typeof(SessionContext))]
	class When_disposing_a_session_context_constructed_with_a_session_func_and_session_has_not_been_accessed : WithSubject<SessionFuncSessionContext>
	{
		It should_not_call_the_func = () =>
			funcExecuteCount.ShouldEqual(0);

		Because of = () =>
			Subject.Dispose();

		Establish context = () =>
			Configure<Func<ISession>>(getSession);

		static ISession getSession()
		{
			funcExecuteCount++;
			return An<ISession>();
		}

		static int funcExecuteCount;
	}

	[Subject(typeof(SessionContext))]
	class When_disposing_a_session_context_constructed_with_a_session_func_and_session_has_been_accessed : WithSubject<SessionFuncSessionContext>
	{
		It should_dispose_the_session_obtained_from_the_func_only_once = () =>
			session.WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			session = Subject.Session;
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
			Configure<Func<ISession>>(getSession);

		static ISession getSession()
		{
			funcExecuteCount++;
			return An<ISession>();
		}

		static int funcExecuteCount;
		static ISession session;
	}

	class SessionFuncSessionContext : SessionContext
	{
		public SessionFuncSessionContext(Func<ISession> sessionFunc) : base(sessionFunc) { }
	}

	[Subject(typeof(SessionContext))]
	class When_getting_session_from_a_session_context_constructed_with_a_lazy_session_scope : WithSubject<LazySessionScopeSessionContext>
	{
		It should_return_the_current_session = () =>
			session.ShouldBeTheSameAs(The<ILazySessionScope>().Current);

		Because of = () =>
			session = Subject.Session;

		Establish context = () =>
			The<ILazySessionScope>().WhenToldTo(x => x.Current).Return(The<ISession>());

		static ISession session;
	}

	[Subject(typeof(SessionContext))]
	class When_disposing_a_session_context_constructed_with_a_lazy_session_scope : WithSubject<LazySessionScopeSessionContext>
	{
		It should_dispose_the_lazy_session_scope_only_once = () =>
			The<ILazySessionScope>().WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
			The<ILazySessionScope>().WhenToldTo(x => x.Current).Return(The<ISession>());
	}

	class LazySessionScopeSessionContext : SessionContext
	{
		public LazySessionScopeSessionContext(ILazySessionScope lazySessionScope) : base(lazySessionScope) { }
	}
}
