using System;
using System.Reflection;
using DemoApplication.SingleSessionFactory.Entities;
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

namespace DemoApplication.SingleSessionFactory
{
	static class SingleSessionFactoryDemo
	{
		public static void Execute()
		{
			Program.EnsureDatabaseExists("NHibernate.Sessions.DemoApplication.SingleSessionFactory");
			const string connectionStringName = "SingleSessionFactory";
			new SchemaExport(getConfiguration(connectionStringName)).Execute(true, true, false);

			Console.WriteLine("Configuring an {0}", typeof(ISingleSessionFactoryConfigurer).Name);
			var singleSessionFactoryConfigurer = ConfigureNHibernate
				.ForSingleSessionFactory(() =>
				{
					Console.WriteLine("Building NHibernate configuration for {0}", connectionStringName);
					return getConfiguration(connectionStringName);
				})
				.WithLazyInitialization()
				.WithInitializedCallback(x => Console.WriteLine("{0} initialized", x.GetType().Name))
				.WithSessionOpenedCallback(x => Console.WriteLine("{0} opened", x.GetType().Name));

			Console.WriteLine("Creating an {0}", typeof(ILazySessionScope).Name);
			var lazySessionScoper = singleSessionFactoryConfigurer.CreateLazySession();
			Console.WriteLine("Creating an {0}", typeof(ILazySessionFactory).Name);
			var lazySessionFactory = singleSessionFactoryConfigurer.CreateLazySessionFactory();
			Console.WriteLine("Creating a {0}", typeof(SingleSessionFactoryScenarios).FullName);
			using (var scenarios = new SingleSessionFactoryScenarios(lazySessionScoper, lazySessionFactory))
			{
				var users = scenarios.Execute(x => x.GetAllUsers());
				Console.WriteLine("Found {0} users", users.Count);

				var todoItems = scenarios.Execute(x => x.GetAllTodoItems(users[0].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.Execute(x => x.GetTodoItemsDueThisWeek(users[1].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.Execute(x => x.GetTodoItemsDueThisWeek(users[0].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.Execute(x => x.GetTodoItemsDueThisMonth(users[1].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.Execute(x => x.GetTodoItemsCompletedLastWeek(users[0].Id));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				todoItems = scenarios.Execute(x => x.GetUpcomingTodoItems(users[1].Id, Priority.Urgent));
				Console.WriteLine("Found {0} todo items", todoItems.Count);

				var success = scenarios.Execute(x => x.CompleteTodoItem(todoItems[0].Id));
				Console.WriteLine("TodoItem {0} {1} completed", todoItems[0].Id, success ? "was" : "wasn't");
			}
		}

		static Configuration getConfiguration(string connectionStringName)
		{
			return Fluently.Configure()
				.Database(MsSqlConfiguration.MsSql2012.ConnectionString(c => c.FromConnectionStringWithKey(connectionStringName)))
				.Mappings(m => m.AutoMappings.Add(AutoMap
					.Source(new AssemblyTypeSource(Assembly.GetExecutingAssembly()))
					.Where(t => typeof(IEntity).IsAssignableFrom(t) && t.Namespace.Contains(connectionStringName))
					.IgnoreBase(typeof(IdentityFieldProvider<,>))
					.Conventions.AddAssembly(Assembly.GetExecutingAssembly())))
				.CurrentSessionContext<ThreadStaticSessionContext>()
				.BuildConfiguration();
		}
	}
}
