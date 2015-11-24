using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NHibernate.Context;

namespace NHibernate.Sessions
{
	public class ThreadLocalSessionContextBinder : ICurrentSessionContextBinder
	{
		static readonly Func<IDictionary<ISessionFactory, ISession>> contextFieldGetter = createContextFieldGetter();

		static readonly Lazy<ThreadLocalSessionContextBinder> _instance = new Lazy<ThreadLocalSessionContextBinder>();
		public static ThreadLocalSessionContextBinder Instance
		{
			get { return _instance.Value; }
		}

		public virtual bool HasBind(ISessionFactory sessionFactory)
		{
			var context = contextFieldGetter();
			return context != null && context.ContainsKey(sessionFactory);
		}

		public virtual ISession BindNew(ISessionFactory sessionFactory)
		{
			return sessionFactory.GetCurrentSession();
		}

		public virtual ISession Unbind(ISessionFactory sessionFactory)
		{
			return ThreadLocalSessionContext.Unbind(sessionFactory);
		}

		static Func<IDictionary<ISessionFactory, ISession>> createContextFieldGetter()
		{
			var field = typeof(ThreadLocalSessionContext).GetField("context", BindingFlags.NonPublic | BindingFlags.Static);
			var methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			var getterMethod = new DynamicMethod(methodName, typeof(IDictionary<ISessionFactory, ISession>), new System.Type[0], true);
			var gen = getterMethod.GetILGenerator();
			gen.Emit(OpCodes.Ldsfld, field);
			gen.Emit(OpCodes.Ret);
			return (Func<IDictionary<ISessionFactory, ISession>>)getterMethod.CreateDelegate(typeof(Func<IDictionary<ISessionFactory, ISession>>));
		}
	}
}