using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Sessions;

namespace DemoApplication.SingleSessionFactory
{
	class SingleSessionFactoryScenarios : IDisposable
	{
		readonly ILazySessionScoper _lazySessionScoper;
		readonly ILazySessionFactory _lazySessionFactory;

		public SingleSessionFactoryScenarios(ILazySessionScoper lazySessionScoper, ILazySessionFactory lazySessionFactory)
		{
			_lazySessionScoper = lazySessionScoper;
			_lazySessionFactory = lazySessionFactory;
			createSampleData();
		}

		void createSampleData()
		{
			Console.WriteLine("Creating sample data for {0}", GetType().Name);
			var sampleTodoItems = SampleDataFactory.CreateTodoItems();
			using (var session = _lazySessionFactory.Value.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				foreach (var sampleUser in sampleTodoItems.Select(x => x.User).Distinct())
					session.Save(sampleUser);
				foreach (var sampleTodoItem in sampleTodoItems)
					session.Save(sampleTodoItem);
				transaction.Commit();
			}
		}

		void deleteAllData()
		{
			Console.WriteLine("Deleting all data for {0}", GetType().Name);
			using (var session = _lazySessionFactory.Value.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete TodoItem t").ExecuteUpdate();
				session.CreateQuery("delete User u").ExecuteUpdate();
				transaction.Commit();
			}
		}

		public void Dispose()
		{
			deleteAllData();
			_lazySessionScoper.Dispose();
			_lazySessionFactory.Dispose();
		}

		public T Execute<T>(Expression<Func<TodoItemsService, T>> scenarioExpression)
		{
			using (var scope = _lazySessionScoper.Scope())
			{
				Console.WriteLine("Creating a {0}", typeof(TodoItemsService).FullName);
				var todoItemsService = new TodoItemsService(scope);
				todoItemsService.Log(scenarioExpression);
				return scenarioExpression.Compile()(todoItemsService);
			}
		}
	}
}