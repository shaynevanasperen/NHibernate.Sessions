using System;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Engine;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_and_the_lazy_session_factory_current_session_context_binder_has_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_not_invoke_the_on_session_opened_callback = () =>
			onSessionOpenedCallbackInvoked.ShouldBeFalse();

		Because of = () =>
			result = Subject.Current;

		Establish context = () =>
		{
			Configure<Action<ISession>>(x => onSessionOpenedCallbackInvoked = true);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactory>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.HasBind(The<ISessionFactory>())).Return(true);
			The<ISessionFactory>().WhenToldTo(x => x.GetCurrentSession()).Return(The<ISession>);
		};

		static ISession result;
		static bool onSessionOpenedCallbackInvoked;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_and_the_lazy_session_factory_current_session_context_binder_does_not_have_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_invoke_the_on_session_opened_callback = () =>
			onSessionOpenedCallbackInvoked.ShouldBeTrue();

		Because of = () =>
			result = Subject.Current;

		Establish context = () =>
		{
			Configure<Action<ISession>>(x => onSessionOpenedCallbackInvoked = x == The<ISession>());
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactory>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.BindNew(The<ISessionFactory>())).Return(The<ISession>);
		};

		static ISession result;
		static bool onSessionOpenedCallbackInvoked;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_from_scope_and_the_lazy_session_factory_current_session_context_binder_has_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_not_invoke_the_on_session_opened_callback = () =>
			onSessionOpenedCallbackInvoked.ShouldBeFalse();

		Because of = () =>
			result = Subject.Scope().Current;

		Establish context = () =>
		{
			Configure<Action<ISession>>(x => onSessionOpenedCallbackInvoked = true);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactory>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.HasBind(The<ISessionFactory>())).Return(true);
			The<ISessionFactory>().WhenToldTo(x => x.GetCurrentSession()).Return(The<ISession>);
		};

		static ISession result;
		static bool onSessionOpenedCallbackInvoked;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_getting_current_from_scope_and_the_lazy_session_factory_current_session_context_binder_does_not_have_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_invoke_the_on_session_opened_callback = () =>
			onSessionOpenedCallbackInvoked.ShouldBeTrue();

		Because of = () =>
			result = Subject.Scope().Current;

		Establish context = () =>
		{
			Configure<Action<ISession>>(x => onSessionOpenedCallbackInvoked = x == The<ISession>());
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactory>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.BindNew(The<ISessionFactory>())).Return(The<ISession>);
		};

		static ISession result;
		static bool onSessionOpenedCallbackInvoked;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_scope_and_the_lazy_session_factory_current_session_context_binder_has_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_unbind_the_current_session_only_once = () =>
			The<ICurrentSessionContextBinder>().WasToldTo(x => x.Unbind(The<ISessionFactoryImplementor>())).OnlyOnce();

		It should_dispose_the_current_session_only_once = () =>
			The<ISession>().WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			var scope = Subject.Scope();
			scope.Dispose();
			scope.Dispose();
		};

		Establish context = () =>
		{
			Configure<Action<ISession>>(null);
			The<ILazySessionFactory>().WhenToldTo(x => x.IsValueCreated).Return(true);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactoryImplementor>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.Unbind(The<ISessionFactoryImplementor>())).Return(The<ISession>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.HasBind(The<ISessionFactoryImplementor>())).Return(true);
		};
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_scope_and_the_lazy_session_factory_current_session_context_binder_does_not_have_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_run_only_once = () =>
			The<ICurrentSessionContextBinder>().WasToldTo(x => x.HasBind(Param<ISessionFactory>.IsAnything)).OnlyOnce();

		It should_not_unbind_anything = () =>
			The<ICurrentSessionContextBinder>().WasNotToldTo(x => x.Unbind(Param<ISessionFactory>.IsAnything));

		Because of = () =>
		{
			var scope = Subject.Scope();
			scope.Dispose();
			scope.Dispose();
		};

		Establish context = () =>
		{
			Configure<Action<ISession>>(null);
			The<ILazySessionFactory>().WhenToldTo(x => x.IsValueCreated).Return(true);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactoryImplementor>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
		};
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_scope_before_the_lazy_session_factory_value_was_created : WithSubject<LazySession>
	{
		It should_run_only_once = () =>
			count.ShouldEqual(1);

		It should_not_check_if_the_current_session_context_is_bound = () =>
			The<ICurrentSessionContextBinder>().WasNotToldTo(x => x.HasBind(Param<ISessionFactory>.IsAnything));

		Because of = () =>
		{
			var scope = Subject.Scope();
			scope.Dispose();
			scope.Dispose();
		};

		Establish context = () =>
		{
			Configure<Action<ISession>>(null);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactoryImplementor>);
			The<ILazySessionFactory>().WhenToldTo(x => x.IsValueCreated).Return(() => ++count == 0);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
		};

		static int count;
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_and_the_lazy_session_factory_current_session_context_binder_has_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_unbind_the_current_session_only_once = () =>
			The<ICurrentSessionContextBinder>().WasToldTo(x => x.Unbind(The<ISessionFactoryImplementor>())).OnlyOnce();

		It should_dispose_the_current_session_only_once = () =>
			The<ISession>().WasToldTo(x => x.Dispose()).OnlyOnce();

		It should_dispose_the_lazy_session_factory_only_once = () =>
			The<ILazySessionFactory>().WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
		{
			Configure<Action<ISession>>(null);
			The<ILazySessionFactory>().WhenToldTo(x => x.IsValueCreated).Return(true);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactoryImplementor>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.Unbind(The<ISessionFactoryImplementor>())).Return(The<ISession>);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.HasBind(The<ISessionFactoryImplementor>())).Return(true);
		};
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_and_the_lazy_session_factory_current_session_context_binder_does_not_have_a_bind_for_the_value : WithSubject<LazySession>
	{
		It should_run_only_once = () =>
			The<ICurrentSessionContextBinder>().WasToldTo(x => x.HasBind(Param<ISessionFactory>.IsAnything)).OnlyOnce();

		It should_not_unbind_anything = () =>
			The<ICurrentSessionContextBinder>().WasNotToldTo(x => x.Unbind(Param<ISessionFactory>.IsAnything));

		It should_dispose_the_lazy_session_factory_only_once = () =>
			The<ILazySessionFactory>().WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
		{
			Configure<Action<ISession>>(null);
			The<ILazySessionFactory>().WhenToldTo(x => x.IsValueCreated).Return(true);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactoryImplementor>);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
		};
	}

	[Subject(typeof(LazySessionFactory))]
	class When_disposing_before_the_lazy_session_factory_value_was_created : WithSubject<LazySession>
	{
		It should_run_only_once = () =>
			count.ShouldEqual(1);

		It should_not_check_if_the_current_session_context_is_bound = () =>
			The<ICurrentSessionContextBinder>().WasNotToldTo(x => x.HasBind(Param<ISessionFactory>.IsAnything));

		It should_dispose_the_lazy_session_factory_only_once = () =>
			The<ILazySessionFactory>().WasToldTo(x => x.Dispose()).OnlyOnce();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
		{
			Configure<Action<ISession>>(null);
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactoryImplementor>);
			The<ILazySessionFactory>().WhenToldTo(x => x.IsValueCreated).Return(() => ++count == 0);
			The<ILazySessionFactory>().WhenToldTo(x => x.CurrentSessionContextBinder).Return(The<ICurrentSessionContextBinder>);
		};

		static int count;
	}
}
