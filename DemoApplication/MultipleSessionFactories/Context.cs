using System;
using NHibernate;
using NHibernate.Sessions;

namespace DemoApplication.MultipleSessionFactories
{
	interface ITodoItemsContext : ISessionContext { }
	interface IUsersContext : ISessionContext { }

	class TodoItemsContext : SessionContext, ITodoItemsContext
	{
		public TodoItemsContext(Func<ISession> sessionFunc) : base(sessionFunc) { }
		public TodoItemsContext(ILazySessionScope lazySessionScope) : base(lazySessionScope) { }
	}

	class UsersContext : SessionContext, IUsersContext
	{
		public UsersContext(Func<ISession> sessionFunc) : base(sessionFunc) { }
		public UsersContext(ILazySessionScope lazySessionScope) : base(lazySessionScope) { }
	}
}
