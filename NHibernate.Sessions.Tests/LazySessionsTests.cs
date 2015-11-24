using System.Collections.Generic;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Util;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(LazySessions))]
	class When_getting_scope_for_a_factory_key : WithSubject<LazySessions>
	{
		It should_return_a_scope_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ILazySessionScope>());

		Because of = () =>
			result = Subject.ScopeFor("key");

		Establish context = () =>
		{
			The<ILazySessionScoper>().WhenToldTo(x => x.Scope()).Return(The<ILazySessionScope>);
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper> { { "key", The<ILazySessionScoper>() } });
		};

		static ILazySessionScope result;
	}

	[Subject(typeof(LazySessions))]
	class When_getting_scope_for_a_factory_key_type : WithSubject<LazySessions>
	{
		It should_return_a_scope_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ILazySessionScope>());

		Because of = () =>
			result = Subject.ScopeFor<FactoryKeyType>();

		Establish context = () =>
		{
			The<ILazySessionScoper>().WhenToldTo(x => x.Scope()).Return(The<ILazySessionScope>);
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper> { { typeof(FactoryKeyType).Name, The<ILazySessionScoper>() } });
		};

		static ILazySessionScope result;

		class FactoryKeyType { }
	}

	[Subject(typeof(LazySessions))]
	class When_getting_current_for_a_factory_key : WithSubject<LazySessions>
	{
		It should_return_the_current_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		Because of = () =>
			result = Subject.CurrentFor("key");

		Establish context = () =>
		{
			The<ILazySessionScoper>().WhenToldTo(x => x.Scope().Current).Return(The<ISession>);
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper> { { "key", The<ILazySessionScoper>() } });
		};

		static ISession result;
	}

	[Subject(typeof(LazySessions))]
	class When_getting_current_for_a_factory_key_type : WithSubject<LazySessions>
	{
		It should_return_the_current_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		Because of = () =>
			result = Subject.CurrentFor<FactoryKeyType>();

		Establish context = () =>
		{
			The<ILazySessionScoper>().WhenToldTo(x => x.Scope().Current).Return(The<ISession>);
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper> { { typeof(FactoryKeyType).Name, The<ILazySessionScoper>() } });
		};

		static ISession result;

		class FactoryKeyType { }
	}

	[Subject(typeof(LazySessions))]
	class When_getting_current_for_a_factory_key_from_scope : WithSubject<LazySessions>
	{
		It should_return_the_current_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		Because of = () =>
			result = Subject.Scope().CurrentFor("key");

		Establish context = () =>
		{
			The<ILazySessionScoper>().WhenToldTo(x => x.Scope().Current).Return(The<ISession>);
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper> { { "key", The<ILazySessionScoper>() } });
		};

		static ISession result;
	}

	[Subject(typeof(LazySessions))]
	class When_getting_current_for_a_factory_key_type_from_scope : WithSubject<LazySessions>
	{
		It should_return_the_current_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		Because of = () =>
			result = Subject.Scope().CurrentFor<FactoryKeyType>();

		Establish context = () =>
		{
			The<ILazySessionScoper>().WhenToldTo(x => x.Scope().Current).Return(The<ISession>);
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper> { { typeof(FactoryKeyType).Name, The<ILazySessionScoper>() } });
		};

		static ISession result;

		class FactoryKeyType { }
	}

	[Subject(typeof(LazySessions))]
	class When_disposing_scope : WithSubject<LazySessions>
	{
		It should_unbind_and_dispose_current_for_all_lazy_session_scopers_only_once = () =>
		{
			foreach (var lazySessionScoper in The<IDictionary<string, ILazySessionScoper>>().Values)
				lazySessionScoper.WasToldTo(x => x.UnbindAndDisposeCurrent()).OnlyOnce();
		};

		It should_not_dispose_any_lazy_session_scopers = () =>
		{
			foreach (var lazySessionScoper in The<IDictionary<string, ILazySessionScoper>>().Values)
				lazySessionScoper.WasNotToldTo(x => x.Dispose());
		};

		Because of = () =>
		{
			var scope = Subject.Scope();
			scope.Dispose();
			scope.Dispose();
		};

		Establish context = () =>
		{
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper>
			{
				{ "1", An<ILazySessionScoper>() },
				{ "2", An<ILazySessionScoper>() }
			});
		};
	}

	[Subject(typeof(LazySessions))]
	class When_disposing_lazy_sessions : WithSubject<LazySessions>
	{
		It should_dispose_all_lazy_session_scopers_only_once = () =>
		{
			foreach (var lazySessionScoper in The<IDictionary<string, ILazySessionScoper>>().Values)
				lazySessionScoper.WasToldTo(x => x.Dispose()).OnlyOnce();
		};

		It should_not_unbind_and_dispose_current_for_any_lazy_session_scopers = () =>
		{
			foreach (var lazySessionScoper in The<IDictionary<string, ILazySessionScoper>>().Values)
				lazySessionScoper.WasNotToldTo(x => x.UnbindAndDisposeCurrent());
		};

		It should_clear_the_lazy_session_scopers = () =>
			The<IDictionary<string, ILazySessionScoper>>().Any().ShouldBeFalse();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
		{
			Configure<IDictionary<string, ILazySessionScoper>>(new Dictionary<string, ILazySessionScoper>
			{
				{ "1", An<ILazySessionScoper>() },
				{ "2", An<ILazySessionScoper>() }
			});
		};
	}
}
