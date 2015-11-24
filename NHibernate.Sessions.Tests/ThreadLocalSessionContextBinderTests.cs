using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.Context;
using NHibernate.Engine;

namespace NHibernate.Sessions.Tests
{
	[Subject(typeof(ThreadLocalSessionContextBinder))]
	class When_calling_hasbind : WithFakes
	{
		It should_be_fast = () =>
		{
			Console.WriteLine("Baseline: {0:N}ns", baselineTime);
			Console.WriteLine("Reflection Initialize: {0:N}ns", reflectionInitializeTime);
			Console.WriteLine("Reflection: {0:N}ns", reflectionTime);
			Console.WriteLine("ThreadLocalSessionContextBinder Initialize: {0:N}ns", threadLocalSessionContextBinderInitializeTime);
			Console.WriteLine("ThreadLocalSessionContextBinder: {0:N}ns", threadLocalSessionContextBinderTime);

			Console.WriteLine("ThreadLocalSessionContextBinder is {0:N3} times slower than Baseline", threadLocalSessionContextBinderTime / baselineTime);
			Console.WriteLine("ThreadLocalSessionContextBinder is {0:N3} times faster than Reflection", reflectionTime / threadLocalSessionContextBinderTime);

			// NOTE: These thresholds are skewed in order to compensate for the test runner interfering with the timings when running the entire test suite
			// The real values are significantly better when running this test on its own
			threadLocalSessionContextBinderTime.ShouldBeLessThan(baselineTime * 3);
			threadLocalSessionContextBinderTime.ShouldBeLessThan(reflectionTime / 2);
		};

		It should_not_share_context_among_threads = () =>
		{
			var hasBindInThread = false;
			var thread = new Thread(() =>
			{
				ThreadLocalSessionContext.Bind(session);
				hasBindInThread = threadLocalSessionContextBinder.HasBind(sessionFactory);
			});
			thread.Start();
			thread.Join();
			hasBindInThread.ShouldBeTrue();
			threadLocalSessionContextBinder.HasBind(sessionFactory).ShouldBeFalse();
		};

		Because of = () =>
		{
			const int iterations = 1000000;
			baselineTime = time(() => Baseline.HasBind(sessionFactory), iterations);
			reflectionInitializeTime = time(() => contextField = typeof(ThreadLocalSessionContext).GetField("context", BindingFlags.NonPublic | BindingFlags.Static), 1);
			reflectionTime = time(() =>
			{
				var context = (IDictionary<ISessionFactory, ISession>)contextField.GetValue(threadLocalSessionContext);
				reflectionHasBind = context != null && context.ContainsKey(sessionFactory);
			}, iterations);
			threadLocalSessionContextBinderInitializeTime = time(() => threadLocalSessionContextBinder = ThreadLocalSessionContextBinder.Instance, 1);
			threadLocalSessionContextBinderTime = time(() => threadLocalSessionContextBinderHasBind = threadLocalSessionContextBinder.HasBind(sessionFactory), iterations);
		};

		Establish context = () =>
		{
			sessionFactory = An<ISessionFactoryImplementor>();
			session = An<ISession>();
			session.WhenToldTo(x => x.SessionFactory).Return(sessionFactory);
			threadLocalSessionContext = new ThreadLocalSessionContext(sessionFactory);
		};

		static double time(System.Action action, int iterations)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			for (var i = 0; i < iterations; i++)
				action();
			return stopwatch.ElapsedTicks / ((double)Stopwatch.Frequency / (1000L * 1000L * 1000L)) / iterations;
		}

		static ISession session;
		static ISessionFactoryImplementor sessionFactory;
		static FieldInfo contextField;
		static ThreadLocalSessionContext threadLocalSessionContext;
		static ThreadLocalSessionContextBinder threadLocalSessionContextBinder;
		static double baselineTime, reflectionInitializeTime, reflectionTime, threadLocalSessionContextBinderInitializeTime, threadLocalSessionContextBinderTime;
		static bool reflectionHasBind, threadLocalSessionContextBinderHasBind;

		class Baseline
		{
			[ThreadStatic]
			static IDictionary<ISessionFactory, ISession> _context;

			public static bool HasBind(ISessionFactory sessionFactory)
			{
				return _context != null && _context.ContainsKey(sessionFactory);
			}

			public static void Bind(ISession session)
			{
				_context = new Dictionary<ISessionFactory, ISession>();
			}
		}
	}
}
