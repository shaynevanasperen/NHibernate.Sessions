using System;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Context;
using NHibernate.Engine;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(SessionManager))]
	class When_a_single_session_factory_has_been_registered : WithSubject<SessionManager>
	{
		It should_throw_an_invalid_operation_exception_on_trying_to_register_a_session_factory_with_a_key_that_already_exists = () =>
			Catch.Exception(() => Subject.RegisterSessionFactory(sessionFactoryEntry)).ShouldBeOfExactType<InvalidOperationException>();

		It should_return_a_session_from_the_registered_session_factory_entry = () =>
		{
			Subject.Session.ShouldBeTheSameAs(sessionFactoryEntry.Session);
			Subject.SessionFor(SessionFactoryKeys.Key1).ShouldBeTheSameAs(sessionFactoryEntry.Session);
			Subject.SessionFor<TypeWithSessionFactoryKey1>().ShouldBeTheSameAs(sessionFactoryEntry.Session);
			Subject.SessionFor<TypeWithoutSessionFactoryKey>().ShouldBeTheSameAs(sessionFactoryEntry.Session);
		};

		It should_return_the_session_factory_from_the_registered_session_factory_entry = () =>
		{
			Subject.SessionFactory.ShouldBeTheSameAs(sessionFactoryEntry.SessionFactory);
			Subject.SessionFactoryFor(SessionFactoryKeys.Key1).ShouldBeTheSameAs(sessionFactoryEntry.SessionFactory);
			Subject.SessionFactoryFor<TypeWithSessionFactoryKey1>().ShouldBeTheSameAs(sessionFactoryEntry.SessionFactory);
			Subject.SessionFactoryFor<TypeWithoutSessionFactoryKey>().ShouldBeTheSameAs(sessionFactoryEntry.SessionFactory);
		};

		It should_throw_an_argument_exception_on_trying_to_get_a_session_for_a_session_factory_key_which_is_not_registered = () =>
		{
			Catch.Exception(() => Subject.SessionFor(SessionFactoryKeys.Key2)).ShouldBeOfExactType<ArgumentException>();
			Catch.Exception(() => Subject.SessionFor<TypeWithSessionFactoryKey2>()).ShouldBeOfExactType<ArgumentException>();
		};

		It should_throw_an_argument_exception_on_trying_to_get_the_session_factory_for_a_session_factory_key_which_is_not_registered = () =>
		{
			Catch.Exception(() => Subject.SessionFactoryFor(SessionFactoryKeys.Key2)).ShouldBeOfExactType<ArgumentException>();
			Catch.Exception(() => Subject.SessionFactoryFor<TypeWithSessionFactoryKey2>()).ShouldBeOfExactType<ArgumentException>();
		};

		It should_report_that_it_does_not_have_multiple_session_factories = () =>
			Subject.HasMultipleSessionFactories.ShouldBeFalse();

		Establish context = () =>
		{
			sessionFactoryEntry = An<ISessionFactoryEntry>();
			sessionFactoryEntry.WhenToldTo(x => x.Key).Return(SessionFactoryKeys.Key1);
			sessionFactoryEntry.WhenToldTo(x => x.SessionFactory).Return(An<ISessionFactory>());
			sessionFactoryEntry.WhenToldTo(x => x.Session).Return(An<ISession>());
			Subject.RegisterSessionFactory(sessionFactoryEntry);
		};

		Cleanup after = () =>
			SessionManager.SessionFactoryEntries.Clear();

		static ISessionFactoryEntry sessionFactoryEntry;
	}

	[Subject(typeof(SessionManager))]
	class When_multiple_session_factories_have_been_registered : WithSubject<SessionManager>
	{
		It should_throw_an_invalid_operation_exception_on_trying_to_register_a_session_factory_with_a_key_that_already_exists = () =>
			Catch.Exception(() => Subject.RegisterSessionFactory(sessionFactoryEntry1)).ShouldBeOfExactType<InvalidOperationException>();

		It should_return_a_session_from_the_corresponding_session_factory_entry = () =>
		{
			Subject.SessionFor(SessionFactoryKeys.Key1).ShouldBeTheSameAs(sessionFactoryEntry1.Session);
			Subject.SessionFor<TypeWithSessionFactoryKey1>().ShouldBeTheSameAs(sessionFactoryEntry1.Session);
		};

		It should_return_the_session_factory_from_the_corresponding_session_factory_entry = () =>
		{
			Subject.SessionFactoryFor(SessionFactoryKeys.Key1).ShouldBeTheSameAs(sessionFactoryEntry1.SessionFactory);
			Subject.SessionFactoryFor<TypeWithSessionFactoryKey1>().ShouldBeTheSameAs(sessionFactoryEntry1.SessionFactory);
		};

		It should_throw_an_invalid_operation_exception_on_trying_to_get_a_session_without_specifying_a_session_factory_key = () =>
		{
			Catch.Exception(() => Subject.Session).ShouldBeOfExactType<InvalidOperationException>();
			Catch.Exception(() => Subject.SessionFor<TypeWithoutSessionFactoryKey>()).ShouldBeOfExactType<InvalidOperationException>();
		};

		It should_throw_an_invalid_operation_exception_on_trying_to_get_the_session_factory_without_specifying_a_session_factory_key = () =>
		{
			Catch.Exception(() => Subject.SessionFactory).ShouldBeOfExactType<InvalidOperationException>();
			Catch.Exception(() => Subject.SessionFactoryFor<TypeWithoutSessionFactoryKey>()).ShouldBeOfExactType<InvalidOperationException>();
		};

		It should_throw_an_argument_exception_on_trying_to_get_a_session_for_a_session_factory_key_which_is_not_registered = () =>
		{
			Catch.Exception(() => Subject.SessionFor(SessionFactoryKeys.Key3)).ShouldBeOfExactType<ArgumentException>();
			Catch.Exception(() => Subject.SessionFor<TypeWithSessionFactoryKey3>()).ShouldBeOfExactType<ArgumentException>();
		};

		It should_throw_an_argument_exception_on_trying_to_get_the_session_factory_for_a_session_factory_key_which_is_not_registered = () =>
		{
			Catch.Exception(() => Subject.SessionFactoryFor(SessionFactoryKeys.Key3)).ShouldBeOfExactType<ArgumentException>();
			Catch.Exception(() => Subject.SessionFactoryFor<TypeWithSessionFactoryKey3>()).ShouldBeOfExactType<ArgumentException>();
		};

		It should_report_that_it_does_have_multiple_session_factories = () =>
			Subject.HasMultipleSessionFactories.ShouldBeTrue();

		Establish context = () =>
		{
			sessionFactoryEntry1 = An<ISessionFactoryEntry>();
			sessionFactoryEntry1.WhenToldTo(x => x.Key).Return(SessionFactoryKeys.Key1);
			sessionFactoryEntry1.WhenToldTo(x => x.SessionFactory).Return(An<ISessionFactory>());
			sessionFactoryEntry1.WhenToldTo(x => x.Session).Return(An<ISession>());
			Subject.RegisterSessionFactory(sessionFactoryEntry1);

			sessionFactoryEntry2 = An<ISessionFactoryEntry>();
			sessionFactoryEntry2.WhenToldTo(x => x.Key).Return(SessionFactoryKeys.Key2);
			sessionFactoryEntry2.WhenToldTo(x => x.SessionFactory).Return(An<ISessionFactory>());
			sessionFactoryEntry2.WhenToldTo(x => x.Session).Return(An<ISession>());
			Subject.RegisterSessionFactory(sessionFactoryEntry2);
		};

		Cleanup after = () =>
			SessionManager.SessionFactoryEntries.Clear();

		static ISessionFactoryEntry sessionFactoryEntry1;
		static ISessionFactoryEntry sessionFactoryEntry2;
	}

	[Subject(typeof(SessionManager))]
	class When_disposing : WithSubject<SessionManager>
	{
		It should_close_session_factories_that_are_initialized = () =>
		{
			sessionFactoryEntry1.SessionFactory.WasToldTo(x => x.Close());
			sessionFactoryEntry2.SessionFactory.WasToldTo(x => x.Close());
		};

		It should_not_try_to_close_session_factories_that_are_not_initialized = () =>
			sessionFactoryEntry3.SessionFactory.WasNotToldTo(x => x.Close());

		It should_unbind_session_factories_that_have_a_session_bound_to_current_session_context = () =>
			CurrentSessionContext.HasBind(sessionFactoryEntry1.SessionFactory).ShouldBeFalse();

		It should_close_sessions_that_were_bound_to_current_session_context = () =>
			sessionFactoryEntry1.Session.WasToldTo(x => x.Close());

		It should_clear_the_session_factory_entries = () =>
			SessionManager.SessionFactoryEntries.ShouldBeEmpty();

		Because of = () =>
			Subject.Dispose();

		Establish context = () =>
		{
			var sessionFactoryEntry1SessionFactory = An<ISessionFactoryImplementor>();
			sessionFactoryEntry1SessionFactory.WhenToldTo(x => x.CurrentSessionContext).Return(new CallSessionContext(sessionFactoryEntry1SessionFactory));
			sessionFactoryEntry1 = An<ISessionFactoryEntry>();
			sessionFactoryEntry1.WhenToldTo(x => x.Key).Return(SessionFactoryKeys.Key1);
			sessionFactoryEntry1.WhenToldTo(x => x.SessionFactory).Return(sessionFactoryEntry1SessionFactory);
			sessionFactoryEntry1.WhenToldTo(x => x.Session).Return(An<ISession>());
			sessionFactoryEntry1.WhenToldTo(x => x.IsInitialized).Return(true);
			Subject.RegisterSessionFactory(sessionFactoryEntry1);

			var sessionFactoryEntry2SessionFactory = An<ISessionFactoryImplementor>();
			sessionFactoryEntry2SessionFactory.WhenToldTo(x => x.CurrentSessionContext).Return(new CallSessionContext(sessionFactoryEntry2SessionFactory));
			sessionFactoryEntry2 = An<ISessionFactoryEntry>();
			sessionFactoryEntry2.WhenToldTo(x => x.Key).Return(SessionFactoryKeys.Key2);
			sessionFactoryEntry2.WhenToldTo(x => x.SessionFactory).Return(sessionFactoryEntry2SessionFactory);
			sessionFactoryEntry2.WhenToldTo(x => x.Session).Return(An<ISession>());
			sessionFactoryEntry2.WhenToldTo(x => x.IsInitialized).Return(true);
			Subject.RegisterSessionFactory(sessionFactoryEntry2);

			var sessionFactoryEntry3SessionFactory = An<ISessionFactoryImplementor>();
			sessionFactoryEntry3SessionFactory.WhenToldTo(x => x.CurrentSessionContext).Return(new CallSessionContext(sessionFactoryEntry3SessionFactory));
			sessionFactoryEntry3 = An<ISessionFactoryEntry>();
			sessionFactoryEntry3.WhenToldTo(x => x.Key).Return(SessionFactoryKeys.Key3);
			sessionFactoryEntry3.WhenToldTo(x => x.SessionFactory).Return(sessionFactoryEntry3SessionFactory);
			sessionFactoryEntry3.WhenToldTo(x => x.Session).Return(An<ISession>());
			sessionFactoryEntry3.WhenToldTo(x => x.IsInitialized).Return(false);
			Subject.RegisterSessionFactory(sessionFactoryEntry3);

			sessionFactoryEntry1.Session.WhenToldTo(x => x.SessionFactory).Return(sessionFactoryEntry1.SessionFactory);
			CurrentSessionContext.Bind(sessionFactoryEntry1.Session);
			CurrentSessionContext.HasBind(sessionFactoryEntry1.SessionFactory).ShouldBeTrue();
		};

		static ISessionFactoryEntry sessionFactoryEntry1;
		static ISessionFactoryEntry sessionFactoryEntry2;
		static ISessionFactoryEntry sessionFactoryEntry3;
	}

	class SessionFactoryKeys
	{
		internal const string Key1 = "Key1";
		internal const string Key2 = "Key2";
		internal const string Key3 = "Key3";
	}

	[SessionFactoryKey(SessionFactoryKeys.Key1)]
	class TypeWithSessionFactoryKey1 { }

	[SessionFactoryKey(SessionFactoryKeys.Key2)]
	class TypeWithSessionFactoryKey2 { }

	[SessionFactoryKey(SessionFactoryKeys.Key3)]
	class TypeWithSessionFactoryKey3 { }

	class TypeWithoutSessionFactoryKey { }
}