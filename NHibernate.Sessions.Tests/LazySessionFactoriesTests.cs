using System.Collections.Generic;
using System.Linq;
using Machine.Fakes;
using Machine.Specifications;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(LazySessionFactories))]
	class When_getting_value_for_a_factory_key : WithSubject<LazySessionFactories>
	{
		It should_return_the_value_for_the_specified_factory = () =>
			result.ShouldBeTheSameAs(The<ISessionFactory>());

		Because of = () =>
			result = Subject.ValueFor("key");

		Establish context = () =>
		{
			The<ILazySessionFactory>().WhenToldTo(x => x.Value).Return(The<ISessionFactory>);
			Configure<IDictionary<string, ILazySessionFactory>>(new Dictionary<string, ILazySessionFactory> { { "key", The<ILazySessionFactory>() } });
		};

		static ISessionFactory result;
	}

	[Subject(typeof(LazySessionFactories))]
	class When_disposing_lazy_session_factories : WithSubject<LazySessionFactories>
	{
		It should_dispose_all_lazy_session_factories_only_once = () =>
		{
			foreach (var lazySessionFactory in The<IDictionary<string, ILazySessionFactory>>().Values)
				lazySessionFactory.WasToldTo(x => x.Dispose()).OnlyOnce();
		};

		It should_clear_the_lazy_session_factories = () =>
			The<IDictionary<string, ILazySessionFactory>>().Any().ShouldBeFalse();

		Because of = () =>
		{
			Subject.Dispose();
			Subject.Dispose();
		};

		Establish context = () =>
		{
			Configure<IDictionary<string, ILazySessionFactory>>(new Dictionary<string, ILazySessionFactory>
			{
				{ "1", An<ILazySessionFactory>() },
				{ "2", An<ILazySessionFactory>() }
			});
		};
	}
}
