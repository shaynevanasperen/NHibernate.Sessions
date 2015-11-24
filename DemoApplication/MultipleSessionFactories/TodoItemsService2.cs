using System;
using System.Collections.Generic;
using System.Linq;
using DemoApplication.MultipleSessionFactories.Entities;
using NHibernate.Linq;

namespace DemoApplication.MultipleSessionFactories
{
	class TodoItemsService2
	{
		readonly ITodoItemsContext _todoItemsContext;
		readonly IUsersContext _usersContext;

		public TodoItemsService2(ITodoItemsContext todoItemsContext, IUsersContext usersContext)
		{
			_todoItemsContext = todoItemsContext;
			_usersContext = usersContext;
		}

		public IReadOnlyList<User> GetAllUsers()
		{
			using (var transaction = _usersContext.Session.BeginTransaction())
			{
				var users = _usersContext.Session.Query<User>().ToList();
				transaction.Commit();
				return users;
			}
		}

		public IReadOnlyList<TodoItem> GetTodoItemsDueThisWeek(Guid userId)
		{
			var endOfWeek = DateTime.Today;
			while (endOfWeek.DayOfWeek != DayOfWeek.Monday)
				endOfWeek = endOfWeek.AddDays(1);
			using (var transaction = _todoItemsContext.Session.BeginTransaction())
			{
				var todoItems = _todoItemsContext.Session
					.Query<TodoItem>()
					.Where(x => x.UserId == userId && x.DueDate <= endOfWeek)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetTodoItemsCompletedLastWeek(Guid userId)
		{
			var lastWeekStart = DateTime.Today.AddDays(-7);
			while (lastWeekStart.DayOfWeek != DayOfWeek.Monday)
				lastWeekStart = lastWeekStart.AddDays(-1);
			using (var transaction = _todoItemsContext.Session.BeginTransaction())
			{
				var todoItems = _todoItemsContext.Session
					.Query<TodoItem>()
					.Where(x => x.UserId == userId && x.DateCompleted >= lastWeekStart && x.DateCompleted < lastWeekStart.AddDays(7))
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetUpcomingTodoItems(Guid userId, Priority priority)
		{
			using (var transaction = _todoItemsContext.Session.BeginTransaction())
			{
				var todoItems = _todoItemsContext.Session
					.Query<TodoItem>()
					.Where(x => x.UserId == userId && !x.DateCompleted.HasValue && x.Priority == priority && x.DueDate >= DateTime.Now)
					.OrderBy(x => x.DueDate).ToList();
				transaction.Commit();
				return todoItems;
			}
		}
	}
}