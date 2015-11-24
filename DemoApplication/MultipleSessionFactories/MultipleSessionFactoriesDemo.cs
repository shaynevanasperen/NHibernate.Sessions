using System;
using System.Reflection;
using System.Threading;
using DemoApplication.MultipleSessionFactories.Entities;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Sessions;
using NHibernate.Sessions.Configuration;
using NHibernate.Tool.hbm2ddl;
using Quarks;

namespace DemoApplication.MultipleSessionFactories
{
	static class MultipleSessionFactoriesDemo
	{
		public static void Execute()
		{
			Program.EnsureDatabaseExists("NHibernate.Sessions.DemoApplication.MultipleSessionFactories.TodoItems");
			Program.EnsureDatabaseExists("NHibernate.Sessions.DemoApplication.MultipleSessionFactories.Users");
			new SchemaExport(getConfiguration(typeof(TodoItems).Name)).Execute(true, true, false);
			new SchemaExport(getConfiguration(typeof(Users).Name)).Execute(true, true, false);

			Console.WriteLine("Configuring an {0} on thread {1}", typeof(IMultiSessionFactoryConfigurer).Name, Thread.CurrentThread.ManagedThreadId);
			var multiSessionFactoryConfigurer = ConfigureNHibernate
				.ForMultipleSessionFactories(x =>
				{
					Console.WriteLine("Building NHibernate configuration for {0}", x);
					return getConfiguration(x);
				})
				.WithCachedConfiguration(Assembly.GetExecutingAssembly())
				.WithBackgroundInitialization(BackgroundInitialization.IgnoreException,
					x => Console.WriteLine("Background Initialization started on thread {0}", x.ManagedThreadId),
					x => Console.WriteLine("Background Initialization completed on thread {0}", x.ManagedThreadId))
				.WithInitializedCallback(x => Console.WriteLine("{0} initialized", x.GetType().Name))
				.WithSessionOpenedCallback(x => Console.WriteLine("{0} opened", x.GetType().Name))
				.RegisterSessionFactory<TodoItems>()
				.RegisterSessionFactory<Users>();

			Console.WriteLine("Creating an {0}", typeof(ILazySessionsScope).Name);
			var lazySessionsScoper = multiSessionFactoryConfigurer.CreateLazySessions();
			Console.WriteLine("Creating an {0}", typeof(ILazySessionFactories).Name);
			var lazySessionFactories = multiSessionFactoryConfigurer.CreateLazySessionFactories();
			Console.WriteLine("Creating a {0}", typeof(MultipleSessionFactoriesScenarios).FullName);
			using (var scenarios = new MultipleSessionFactoriesScenarios(lazySessionsScoper, lazySessionFactories))
			{
				Console.WriteLine("Executing various scenarios by using different service types to demonstrate the different usage patterns");

				var users = scenarios.ExecuteWithLazySessions(x => x.GetAllUsers());
				Console.WriteLine("Found {0} users", users.Count);
				users = scenarios.ExecuteWithSessionFuncContexts(x => x.GetAllUsers());
				Console.WriteLine("Found {0} users", users.Count);
				users = scenarios.ExecuteWithLazySessionScopeContexts(x => x.GetAllUsers());
				Console.WriteLine("Found {0} users", users.Count);

				var todoItems = scenarios.ExecuteWithLazySessions(x => x.GetAllTodoItems(users[0].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.ExecuteWithSessionFuncContexts(x => x.GetTodoItemsDueThisWeek(users[1].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.ExecuteWithLazySessionScopeContexts(x => x.GetTodoItemsDueThisWeek(users[0].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.ExecuteWithLazySessions(x => x.GetTodoItemsDueThisMonth(users[1].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.ExecuteWithSessionFuncContexts(x => x.GetTodoItemsCompletedLastWeek(users[0].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.ExecuteWithLazySessionScopeContexts(x => x.GetUpcomingTodoItems(users[1].Id, Priority.Urgent));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				var success = scenarios.ExecuteWithLazySessions(x => x.CompleteTodoItem(todoItems[0].Id));
				Console.WriteLine("TodoItem {0} {1} completed", todoItems[0].Id, success ? "was" : "wasn't");
			}
		}

		static Configuration getConfiguration(string connectionStringName)
		{
			return Fluently.Configure()
				.Database(MsSqlConfiguration.MsSql2012.ConnectionString(c => c.FromConnectionStringWithKey(connectionStringName)))
				.Mappings(m => m.AutoMappings.Add(AutoMap
					.Source(new AssemblyTypeSource(Assembly.GetExecutingAssembly()))
					.Where(t => typeof(IEntity).IsAssignableFrom(t) && t.IsForSessionFactory(connectionStringName))
					.IgnoreBase(typeof(IdentityFieldProvider<,>))
					.Conventions.AddAssembly(Assembly.GetExecutingAssembly())))
				.CurrentSessionContext<ThreadLocalSessionContext>()
				.BuildConfiguration();
		}
	}
}
