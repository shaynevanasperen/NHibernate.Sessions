using System;
using System.Collections.Generic;
using System.Linq;
using DemoApplication.SingleSessionFactory.Entities;
using NHibernate.Linq;
using NHibernate.Sessions;

namespace DemoApplication.SingleSessionFactory
{
	class TodoItemsService
	{
		readonly ILazySession _lazySession;

		public TodoItemsService(ILazySession lazySession)
		{
			_lazySession = lazySession;
		}

		public IReadOnlyList<User> GetAllUsers()
		{
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var users = _lazySession.Current
					.Query<User>()
					.ToList();
				transaction.Commit();
				return users;
			}
		}

		public IReadOnlyList<TodoItem> GetAllTodoItems(int userId)
		{
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var todoItems = _lazySession.Current
					.Query<TodoItem>()
					.Where(x => x.User.Id == userId)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetTodoItemsDueThisWeek(int userId)
		{
			var endOfWeek = DateTime.Today;
			while (endOfWeek.DayOfWeek != DayOfWeek.Monday)
				endOfWeek = endOfWeek.AddDays(1);
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var todoItems = _lazySession.Current
					.Query<TodoItem>()
					.Where(x => x.User.Id == userId && x.DueDate <= endOfWeek)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetTodoItemsDueThisMonth(int userId)
		{
			var today = DateTime.Today;
			var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var todoItems = _lazySession.Current
					.Query<TodoItem>()
					.Where(x => x.User.Id == userId && x.DueDate <= endOfMonth)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetTodoItemsCompletedLastWeek(int userId)
		{
			var lastWeekStart = DateTime.Today.AddDays(-7);
			while (lastWeekStart.DayOfWeek != DayOfWeek.Monday)
				lastWeekStart = lastWeekStart.AddDays(-1);
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var todoItems = _lazySession.Current
					.Query<TodoItem>()
					.Where(x => x.User.Id == userId && x.DateCompleted >= lastWeekStart && x.DateCompleted < lastWeekStart.AddDays(7))
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetUpcomingTodoItems(int userId, Priority priority)
		{
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var todoItems = _lazySession.Current
					.Query<TodoItem>()
					.Where(x => x.User.Id == userId && !x.DateCompleted.HasValue && x.Priority == priority && x.DueDate >= DateTime.Now)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public bool CompleteTodoItem(int id)
		{
			using (var transaction = _lazySession.Current.BeginTransaction())
			{
				var completedCount = _lazySession.Current
					.CreateQuery("update TodoItem t set t.DateCompleted = :dateCompleted where t.Id = :id and t.DateCompleted = null")
					.SetInt32("id", id)
					.SetDateTime("dateCompleted", DateTime.Now)
					.ExecuteUpdate();
				transaction.Commit();
				return completedCount == 1;
			}
		}
	}
}