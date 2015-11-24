using System;
using System.ComponentModel.DataAnnotations;
using Quarks;

namespace DemoApplication.MultipleSessionFactories.Entities
{
	public class TodoItem : IdentityFieldProvider<TodoItem, int>, TodoItems
	{
		protected TodoItem() { }

		public TodoItem(Guid userId, string name, DateTime dueDate) : this()
		{
			UserId = userId;
			Name = name;
			DueDate = dueDate;
		}

		public TodoItem(Guid userId, string name) : this(userId, name, DateTime.Now.AddDays(7)) { }

		[Required]
		public virtual Guid UserId { get; protected set; }

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
