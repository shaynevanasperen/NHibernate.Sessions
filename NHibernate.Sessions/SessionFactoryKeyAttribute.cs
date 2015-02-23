using System;
using Remotion.Linq.Utilities;

namespace NHibernate.Sessions
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SessionFactoryKeyAttribute : Attribute
	{
		public SessionFactoryKeyAttribute(string key)
		{
			if (string.IsNullOrWhiteSpace(key)) throw new ArgumentEmptyException("key");
			Key = key;
		}

		public string Key { get; private set; }
	}
}
