using System;
using System.Linq.Expressions;
using NHibernate.Sessions;

namespace DemoApplication.MultipleSessionFactories
{
	class MultipleSessionFactoriesScenarios : IDisposable
	{
		readonly ILazySessionsScoper _lazySessionsScoper;
		readonly ILazySessionFactories _lazySessionFactories;

		public MultipleSessionFactoriesScenarios(ILazySessionsScoper lazySessionsScoper, ILazySessionFactories lazySessionFactories)
		{
			_lazySessionsScoper = lazySessionsScoper;
			_lazySessionFactories = lazySessionFactories;
			createSampleData();
		}

		void createSampleData()
		{
			Console.WriteLine("Creating sample data in {0} for {1}", typeof(Users).Name, GetType().Name);
			var users = SampleDataFactory.CreateUsers();
			using (var session = _lazySessionFactories.ValueFor<Users>().OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(users.Item1);
				session.Save(users.Item2);
				transaction.Commit();
			}
			Console.WriteLine("Creating sample data in {0} for {1}", typeof(TodoItems).Name, GetType().Name);
			var sampleTodoItems = SampleDataFactory.CreateTodoItems(users.Item1.Id, users.Item2.Id);
			using (var session = _lazySessionFactories.ValueFor<TodoItems>().OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				foreach (var sampleTodoItem in sampleTodoItems)
					session.Save(sampleTodoItem);
				transaction.Commit();
			}
		}

		void deleteAllData()
		{
			Console.WriteLine("Deleting all data in {0} for {1}", typeof(TodoItems).Name, GetType().Name);
			using (var session = _lazySessionFactories.ValueFor<TodoItems>().OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete TodoItem t").ExecuteUpdate();
				transaction.Commit();
			}
			Console.WriteLine("Deleting all data in {0} for {1}", typeof(Users).Name, GetType().Name);
			using (var session = _lazySessionFactories.ValueFor<Users>().OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete User u").ExecuteUpdate();
				transaction.Commit();
			}
		}

		public void Dispose()
		{
			deleteAllData();
			_lazySessionsScoper.Dispose();
			_lazySessionFactories.Dispose();
		}

		public T ExecuteWithLazySessions<T>(Expression<Func<TodoItemsService1, T>> scenarioExpression)
		{
			using (var scope = _lazySessionsScoper.Scope())
			{
				Console.WriteLine("Creating a {0}", typeof(TodoItemsService1).FullName);
				var todoItemsService = new TodoItemsService1(scope);
				todoItemsService.Log(scenarioExpression);
				return scenarioExpression.Compile()(todoItemsService);
			}
		}

		public T ExecuteWithSessionFuncContexts<T>(Expression<Func<TodoItemsService2, T>> scenarioExpression)
		{
			using (var todoItemsContext = new TodoItemsContext(() => _lazySessionFactories.ValueFor<TodoItems>().OpenSession()))
			using (var usersContext = new UsersContext(() => _lazySessionFactories.ValueFor<Users>().OpenSession()))
			{
				return executeWithContexts(scenarioExpression, todoItemsContext, usersContext);
			}
		}

		public T ExecuteWithLazySessionScopeContexts<T>(Expression<Func<TodoItemsService2, T>> scenarioExpression)
		{
			using (var todoItemsContext = new TodoItemsContext(_lazySessionsScoper.ScopeFor<TodoItems>()))
			using (var usersContext = new UsersContext(_lazySessionsScoper.ScopeFor<Users>()))
			{
				return executeWithContexts(scenarioExpression, todoItemsContext, usersContext);
			}
		}

		static T executeWithContexts<T>(Expression<Func<TodoItemsService2, T>> scenarioExpression, ITodoItemsContext todoItemsContext, IUsersContext usersContext)
		{
			Console.WriteLine("Creating a {0}", typeof(TodoItemsService2).FullName);
			var todoItemsService = new TodoItemsService2(todoItemsContext, usersContext);
			todoItemsService.Log(scenarioExpression);
			return scenarioExpression.Compile()(todoItemsService);
		}
	}
}