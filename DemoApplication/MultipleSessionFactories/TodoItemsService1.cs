using System;
using System.Collections.Generic;
using System.Linq;
using DemoApplication.MultipleSessionFactories.Entities;
using NHibernate.Linq;
using NHibernate.Sessions;

namespace DemoApplication.MultipleSessionFactories
{
	class TodoItemsService1
	{
		readonly ILazySessions _lazySessions;

		public TodoItemsService1(ILazySessions lazySessions)
		{
			_lazySessions = lazySessions;
		}

		public IReadOnlyList<User> GetAllUsers()
		{
			using (var transaction = _lazySessions.CurrentFor<Users>().BeginTransaction())
			{
				var users = _lazySessions.CurrentFor<Users>()
					.Query<User>()
					.ToList();
				transaction.Commit();
				return users;
			}
		}

		public IReadOnlyList<TodoItem> GetAllTodoItems(Guid userId)
		{
			using (var transaction = _lazySessions.CurrentFor<TodoItems>().BeginTransaction())
			{
				var todoItems = _lazySessions.CurrentFor<TodoItems>()
					.Query<TodoItem>()
					.Where(x => x.UserId == userId)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public IReadOnlyList<TodoItem> GetTodoItemsDueThisMonth(Guid userId)
		{
			var today = DateTime.Today;
			var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
			using (var transaction = _lazySessions.CurrentFor<TodoItems>().BeginTransaction())
			{
				var todoItems = _lazySessions.CurrentFor<TodoItems>()
					.Query<TodoItem>()
					.Where(x => x.UserId == userId && x.DueDate <= endOfMonth)
					.ToList();
				transaction.Commit();
				return todoItems;
			}
		}

		public bool CompleteTodoItem(int id)
		{
			using (var transaction = _lazySessions.CurrentFor<TodoItems>().BeginTransaction())
			{
				var completedCount = _lazySessions.CurrentFor<TodoItems>()
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