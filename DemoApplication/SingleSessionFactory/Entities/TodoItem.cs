using System;
using System.ComponentModel.DataAnnotations;
using Quarks;

namespace DemoApplication.SingleSessionFactory.Entities
{
	public class TodoItem : IdentityFieldProvider<TodoItem, int>
	{
		protected TodoItem() { }

		public TodoItem(User user, string name, DateTime dueDate) : this()
		{
			User = user;
			Name = name;
			DueDate = dueDate;
		}

		public TodoItem(User user, string name) : this(user, name, DateTime.Now.AddDays(7)) { }

		[Required]
		public virtual User User { get; protected set; }

		[Required]
		public virtual string Name { get; set; }

		public virtual string Details { get; set; }

		public virtual Priority Priority { get; set; }

		public virtual DateTime DueDate { get; set; }

		public virtual DateTime? DateCompleted { get; set; }
	}

	public enum Priority
	{
		Normal,
		Urgent,
		CanWait
	}
}
