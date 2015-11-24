using System.ComponentModel.DataAnnotations;
using Quarks;

namespace DemoApplication.SingleSessionFactory.Entities
{
	public class User : IdentityFieldProvider<User, int>
	{
		protected User() { }

		public User(string name) : this()
		{
			Name = name;
		}

		[Required]
		public virtual string Name { get; set; }
	}
}
