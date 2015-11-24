using System;
using System.Collections.Generic;
using DemoApplication.MultipleSessionFactories.Entities;

namespace DemoApplication.MultipleSessionFactories
{
	static class SampleDataFactory
	{
		public static IReadOnlyList<TodoItem> CreateTodoItems(Guid user1Id, Guid user2Id)
		{
			return new[]
			{
				new TodoItem(user1Id, "1", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user2Id, "2", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user1Id, "3", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user2Id, "4", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user1Id, "5", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user2Id, "6", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user1Id, "7", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user2Id, "8", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user1Id, "9", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user2Id, "10", DateTime.Now.AddYears(-1).AddDays(-1)),
				new TodoItem(user1Id, "1", DateTime.Now.AddDays(-1)) { Id = 1, Priority = Priority.CanWait, DateCompleted = DateTime.Now.AddDays(-13) },
				new TodoItem(user2Id, "2", DateTime.Now.AddDays(-1)) { Id = 2, Priority = Priority.Normal, DateCompleted = DateTime.Now.AddDays(-13) },
				new TodoItem(user1Id, "3", DateTime.Now.AddDays(-2)) { Id = 3, Priority = Priority.Urgent },
				new TodoItem(user2Id, "4", DateTime.Now.AddDays(-2)) { Id = 4, Priority = Priority.CanWait },
				new TodoItem(user1Id, "5", DateTime.Now.AddDays(-3)) { Id = 5, Priority = Priority.Normal, DateCompleted = DateTime.Now.AddDays(-11) },
				new TodoItem(user2Id, "6", DateTime.Now.AddDays(-4)) { Id = 6, Priority = Priority.Urgent, DateCompleted = DateTime.Now.AddDays(-11) },
				new TodoItem(user1Id, "7", DateTime.Now.AddDays(-5)) { Id = 7, Priority = Priority.CanWait },
				new TodoItem(user2Id, "8", DateTime.Now.AddDays(-6)) { Id = 8, Priority = Priority.Normal },
				new TodoItem(user1Id, "9", DateTime.Now.AddDays(-7)) { Id = 9, Priority = Priority.Urgent, DateCompleted = DateTime.Now.AddDays(-9) },
				new TodoItem(user2Id, "10", DateTime.Now.AddDays(0)) { Id = 10, Priority = Priority.CanWait, DateCompleted = DateTime.Now.AddDays(-9) },
				new TodoItem(user1Id, "11", DateTime.Now.AddDays(1)) { Id = 11, Priority = Priority.Normal },
				new TodoItem(user2Id, "12", DateTime.Now.AddDays(2)) { Id = 12, Priority = Priority.Urgent },
				new TodoItem(user1Id, "13", DateTime.Now.AddDays(3)) { Id = 13, Priority = Priority.CanWait, DateCompleted = DateTime.Now.AddDays(-7) },
				new TodoItem(user2Id, "14", DateTime.Now.AddDays(4)) { Id = 14, Priority = Priority.Normal, DateCompleted = DateTime.Now.AddDays(-7) },
				new TodoItem(user1Id, "15", DateTime.Now.AddDays(5)) { Id = 15, Priority = Priority.Urgent },
				new TodoItem(user2Id, "16", DateTime.Now.AddDays(6)) { Id = 16, Priority = Priority.CanWait },
				new TodoItem(user1Id, "17", DateTime.Now.AddDays(7)) { Id = 17, Priority = Priority.Normal, DateCompleted = DateTime.Now.AddDays(-5) },
				new TodoItem(user2Id, "18", DateTime.Now.AddDays(8)) { Id = 18, Priority = Priority.Urgent, DateCompleted = DateTime.Now.AddDays(-5) },
				new TodoItem(user1Id, "19", DateTime.Now.AddDays(9)) { Id = 19, Priority = Priority.CanWait },
				new TodoItem(user2Id, "20", DateTime.Now.AddDays(10)) { Id = 20, Priority = Priority.Normal },
				new TodoItem(user1Id, "21", DateTime.Now.AddDays(11)) { Id = 21, Priority = Priority.Urgent, DateCompleted = DateTime.Now.AddDays(-3) },
				new TodoItem(user2Id, "22", DateTime.Now.AddDays(12)) { Id = 22, Priority = Priority.CanWait, DateCompleted = DateTime.Now.AddDays(-3) },
				new TodoItem(user1Id, "23", DateTime.Now.AddDays(13)) { Id = 23, Priority = Priority.Normal },
				new TodoItem(user2Id, "24", DateTime.Now.AddDays(14)) { Id = 24, Priority = Priority.Urgent },
				new TodoItem(user1Id, "25", DateTime.Now.AddDays(15)) { Id = 25, Priority = Priority.CanWait, DateCompleted = DateTime.Now.AddDays(-1) },
				new TodoItem(user2Id, "26", DateTime.Now.AddDays(16)) { Id = 26, Priority = Priority.Normal, DateCompleted = DateTime.Now.AddDays(-1) },
				new TodoItem(user1Id, "27", DateTime.Now.AddDays(17)) { Id = 27, Priority = Priority.Urgent },
				new TodoItem(user2Id, "28", DateTime.Now.AddDays(18)) { Id = 28, Priority = Priority.CanWait },
				new TodoItem(user1Id, "29", DateTime.Now.AddDays(19)) { Id = 29, Priority = Priority.Normal },
				new TodoItem(user2Id, "30", DateTime.Now.AddDays(20)) { Id = 30, Priority = Priority.Urgent },
				new TodoItem(user1Id, "31", DateTime.Now.AddDays(21)) { Id = 31, Priority = Priority.CanWait },
				new TodoItem(user2Id, "32", DateTime.Now.AddDays(22)) { Id = 32, Priority = Priority.Normal },
				new TodoItem(user1Id, "33", DateTime.Now.AddDays(23)) { Id = 33, Priority = Priority.Urgent },
				new TodoItem(user2Id, "34", DateTime.Now.AddDays(24)) { Id = 34, Priority = Priority.CanWait },
				new TodoItem(user1Id, "35", DateTime.Now.AddDays(25)) { Id = 35, Priority = Priority.Normal },
				new TodoItem(user2Id, "36", DateTime.Now.AddDays(26)) { Id = 36, Priority = Priority.Urgent },
				new TodoItem(user1Id, "37", DateTime.Now.AddDays(27)) { Id = 37, Priority = Priority.CanWait },
				new TodoItem(user2Id, "38", DateTime.Now.AddDays(28)) { Id = 38, Priority = Priority.Normal },
				new TodoItem(user1Id, "39", DateTime.Now.AddDays(29)) { Id = 39, Priority = Priority.Urgent },
				new TodoItem(user2Id, "40", DateTime.Now.AddDays(30)) { Id = 40, Priority = Priority.CanWait },
				new TodoItem(user1Id, "41", DateTime.Now.AddDays(31)) { Id = 41, Priority = Priority.Normal },
				new TodoItem(user2Id, "42", DateTime.Now.AddDays(32)) { Id = 42, Priority = Priority.Urgent },
				new TodoItem(user1Id, "43", DateTime.Now.AddDays(33)) { Id = 43, Priority = Priority.CanWait },
				new TodoItem(user2Id, "44", DateTime.Now.AddDays(34)) { Id = 44, Priority = Priority.Normal },
				new TodoItem(user1Id, "45", DateTime.Now.AddDays(35)) { Id = 45, Priority = Priority.Urgent },
				new TodoItem(user2Id, "46", DateTime.Now.AddDays(36)) { Id = 46, Priority = Priority.CanWait },
				new TodoItem(user1Id, "47", DateTime.Now.AddDays(37)) { Id = 47, Priority = Priority.Normal },
				new TodoItem(user2Id, "48", DateTime.Now.AddDays(38)) { Id = 48, Priority = Priority.Urgent },
				new TodoItem(user1Id, "49", DateTime.Now.AddDays(39)) { Id = 49, Priority = Priority.CanWait },
				new TodoItem(user2Id, "50", DateTime.Now.AddDays(40)) { Id = 50, Priority = Priority.Normal },
				new TodoItem(user1Id, "51", DateTime.Now.AddDays(41)) { Id = 51, Priority = Priority.Urgent },
				new TodoItem(user2Id, "52", DateTime.Now.AddDays(42)) { Id = 52, Priority = Priority.CanWait },
				new TodoItem(user1Id, "53", DateTime.Now.AddDays(43)) { Id = 53, Priority = Priority.Normal },
				new TodoItem(user2Id, "54", DateTime.Now.AddDays(44)) { Id = 54, Priority = Priority.Urgent },
				new TodoItem(user1Id, "55", DateTime.Now.AddDays(45)) { Id = 55, Priority = Priority.CanWait },
				new TodoItem(user2Id, "56", DateTime.Now.AddDays(46)) { Id = 56, Priority = Priority.Normal },
				new TodoItem(user1Id, "57", DateTime.Now.AddDays(47)) { Id = 57, Priority = Priority.Urgent },
				new TodoItem(user2Id, "58", DateTime.Now.AddDays(48)) { Id = 58, Priority = Priority.CanWait },
				new TodoItem(user1Id, "59", DateTime.Now.AddDays(49)) { Id = 59, Priority = Priority.Normal },
				new TodoItem(user2Id, "60", DateTime.Now.AddDays(50)) { Id = 60, Priority = Priority.Urgent }
			};
		}

		public static Tuple<User, User> CreateUsers()
		{
			return new Tuple<User, User>(new User("Bob"), new User("Sue"));
		}
	}
}
