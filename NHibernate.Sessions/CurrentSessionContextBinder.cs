using System;
using NHibernate.Context;

namespace NHibernate.Sessions
{
	public class CurrentSessionContextBinder : ICurrentSessionContextBinder
	{
		static readonly Lazy<CurrentSessionContextBinder> _instance = new Lazy<CurrentSessionContextBinder>();
		public static CurrentSessionContextBinder Instance
		{
			get { return _instance.Value; }
		}

		public virtual bool HasBind(ISessionFactory sessionFactory)
		{
			return CurrentSessionContext.HasBind(sessionFactory);
		}

		public virtual ISession BindNew(ISessionFactory sessionFactory)
		{
			var session = sessionFactory.OpenSession();
			CurrentSessionContext.Bind(session);
			return session;
		}

		public virtual ISession Unbind(ISessionFactory sessionFactory)
		{
			return CurrentSessionContext.Unbind(sessionFactory);
		}
	}
}