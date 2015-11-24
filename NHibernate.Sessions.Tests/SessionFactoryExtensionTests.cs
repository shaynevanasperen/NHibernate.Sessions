using System;
using System.Data;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Engine;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_and_providing_a_current_session_context_binder_and_on_session_opened_callback_and_there_is_a_bind : WithFakes
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_not_invoke_the_on_session_opened_callback = () =>
			onSessionOpenedCallbackInvoked.ShouldBeFalse();

		Because of = () =>
			result = The<ISessionFactory>().GetCurrentOrNewSession(The<ICurrentSessionContextBinder>(), The<Action<ISession>>());

		Establish context = () =>
		{
			Configure<Action<ISession>>(x => onSessionOpenedCallbackInvoked = true);
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.HasBind(The<ISessionFactory>())).Return(true);
			The<ISessionFactory>().WhenToldTo(x => x.GetCurrentSession()).Return(The<ISession>);
		};

		static ISession result;
		static bool onSessionOpenedCallbackInvoked;
	}

	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_and_providing_a_current_session_context_binder_and_on_session_opened_callback_and_there_isnt_a_bind : WithFakes
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_invoke_the_on_session_opened_callback = () =>
			onSessionOpenedCallbackInvoked.ShouldBeTrue();

		Because of = () =>
			result = The<ISessionFactory>().GetCurrentOrNewSession(The<ICurrentSessionContextBinder>(), The<Action<ISession>>());

		Establish context = () =>
		{
			Configure<Action<ISession>>(x => onSessionOpenedCallbackInvoked = x == The<ISession>());
			The<ICurrentSessionContextBinder>().WhenToldTo(x => x.BindNew(The<ISessionFactory>())).Return(The<ISession>);
		};

		static ISession result;
		static bool onSessionOpenedCallbackInvoked;
	}

	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_without_providing_a_current_session_context_binder_or_on_session_opened_callback_and_the_session_factory_does_not_implement_isessionfactoryimplementor : WithFakes
	{
		It should_throw_a_hibernate_exception_stating_that_the_session_factory_does_not_implement_isessionfactoryimplementor = () =>
		{
			exception.ShouldBeOfExactType<HibernateException>();
			exception.Message.ShouldEqual("Session factory does not implement ISessionFactoryImplementor.");
		};

		Because of = () =>
			exception = Catch.Exception(() => The<ISessionFactory>().GetCurrentOrNewSession());

		static Exception exception;
	}

	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_without_providing_a_current_session_context_binder_or_on_session_opened_callback_and_the_session_factory_has_no_current_session_context : WithFakes
	{
		It should_throw_a_hibernate_exception_stating_that_no_curren_session_context_is_configured = () =>
		{
			exception.ShouldBeOfExactType<HibernateException>();
			exception.Message.ShouldEqual("No current session context configured.");
		};

		Because of = () =>
			exception = Catch.Exception(() => The<ISessionFactoryImplementor>().GetCurrentOrNewSession());

		static Exception exception;
	}

	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_without_providing_a_current_session_context_binder_or_on_session_opened_callback_and_the_session_factory_is_configured_for_current_session_context : WithFakes
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_use_the_default_current_session_context_binder = () =>
			CurrentSessionContextBinder.Instance.HasBind(The<ISessionFactory>()).ShouldBeTrue();

		Because of = () =>
			result = The<ISessionFactory>().GetCurrentOrNewSession();

		Establish context = () =>
		{
			The<ISessionFactoryImplementor>().WhenToldTo(x => x.CurrentSessionContext).Return(new ThreadStaticSessionContext(The<ISessionFactoryImplementor>()));
			Configure<ISessionFactory>(The<ISessionFactoryImplementor>());
			The<ISession>().WhenToldTo(x => x.SessionFactory).Return(The<ISessionFactory>);
			The<ISessionFactory>().WhenToldTo(x => x.OpenSession()).Return(The<ISession>);
		};

		static ISession result;
	}

	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_without_providing_a_current_session_context_binder_or_on_session_opened_callback_and_the_session_factory_is_configured_for_thread_local_session_context : WithFakes
	{
		It should_return_the_current_session = () =>
			result.ShouldBeTheSameAs(The<ISession>());

		It should_use_the_default_current_session_context_binder = () =>
			ThreadLocalSessionContextBinder.Instance.HasBind(The<ISessionFactory>()).ShouldBeTrue();

		Because of = () =>
			result = The<ISessionFactory>().GetCurrentOrNewSession();

		Establish context = () =>
		{
			var threadLocalSessionContext = new ThreadLocalSessionContext(The<ISessionFactoryImplementor>());
			The<ISessionFactoryImplementor>().WhenToldTo(x => x.Settings).Return(new Settings());
			The<ISessionFactoryImplementor>().WhenToldTo(x => x.CurrentSessionContext).Return(threadLocalSessionContext);
			The<ISessionFactoryImplementor>()
				.WhenToldTo(x => x.OpenSession(Param<IDbConnection>.IsAnything, Param<bool>.IsAnything, Param<bool>.IsAnything, Param<ConnectionReleaseMode>.IsAnything))
				.Return(The<ISession>);
			The<ISessionFactoryImplementor>().WhenToldTo(x => x.GetCurrentSession()).Return(() => threadLocalSessionContext.CurrentSession());
			Configure<ISessionFactory>(The<ISessionFactoryImplementor>());
		};

		static ISession result;
	}

	[Subject(typeof(SessionFactoryExtension))]
	class When_geting_current_or_new_session_without_providing_a_current_session_context_binder_or_on_session_opened_callback_and_the_session_factory_is_configured_for_custom_session_context : WithFakes
	{
		It should_throw_a_nibernate_exception_stating_that_a_default_current_session_context_binder_could_not_be_provided_as_the_configured_current_session_context_is_not_compatible_with_either_current_session_context_or_thread_local_session_context = () =>
		{
			exception.ShouldBeOfExactType<HibernateException>();
			exception.Message.ShouldEqual("Could not provide a default ICurrentSessionContextBinder " +
										  "as the configured current session context is not compatible " +
										  "with either CurrentSessionContext or ThreadLocalSessionContext.");
		};

		Because of = () =>
			exception = Catch.Exception(() => The<ISessionFactoryImplementor>().GetCurrentOrNewSession());

		Establish context = () =>
			The<ISessionFactoryImplementor>().WhenToldTo(x => x.CurrentSessionContext).Return(new CustomCurrentSessionContext(null));

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
}
