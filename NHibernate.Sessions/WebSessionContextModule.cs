using System;
using System.Linq;
using System.Web;
using NHibernate.Context;
using NHibernate.Util;
using Quarks.NHibernate.ISessionFactoryExtensions;

namespace NHibernate.Sessions
{
	public class WebSessionContextModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.EndRequest += endRequest;
		}

		static void endRequest(object sender, EventArgs e)
		{
			SessionManager.SessionFactoryEntries.Values
				.Where(x => x.IsInitialized &&
				            x.SessionFactory.HasCurrentSessionContext<WebSessionContext>() &&
				            CurrentSessionContext.HasBind(x.SessionFactory))
				.ForEach(x => CurrentSessionContext.Unbind(x.SessionFactory).Close());
		}

		public void Dispose() { }
	}
}
