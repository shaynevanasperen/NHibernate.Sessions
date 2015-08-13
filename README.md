# NHibernate.Sessions

A helpful library for managing multiple NHibernate session factories so that you can communicate with
multiple databases via a single interface. Also supports cached NHibernate configuration and lazy/threaded
session factory initialization for quicker application startup, as well as an initialization callback for
hooking up custom code for logging or integration with third party libraries such as Glimpse.

##Configuration
Here's how to configure a session manager during application startup (or even on the fly).

###1. Multiple databases

You can configure multiple databases like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure(string.Format("~/nhibernate.{0}.cfg.xml", sessionFactoryKey)))
			.RegisterSessionFactory("Database1")
			.RegisterSessionFactory("Database2")
			.BuildSessionManager();
`````

or using FluentNHibernate for one database and xml for another database:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => Fluently.Configure(new Cfg.Configuration())
				.Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey(sessionFactoryKey)))
				.Mappings(m => m.AutoMappings.Add(AutoMap.Source(new AssemblyTypeSource(Assembly.GetExecutingAssembly()))))
				.BuildConfiguration())
			.RegisterSessionFactory("Database1")
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure("~/nhibernate.database2.cfg.xml"))
			.RegisterSessionFactory("Database2")
			.BuildSessionManager();
`````

Obviously you can go wild here and dynamically build the configuration based on the session factory key.

###2. Configuration caching

During an application's startup NHibernate can take significant time when configuring and validating its
mapping to a database. Caching the NHibernate configuration data can reduce initial startup time by storing
the configuration to a file and avoiding the validation checks that run when a configuration is created
from scratch.

The default cache implementation is a file cache which serializes the configuration to a file.
For this to work, all objects contained within the NHibernate configuration must be serializable. All of
the default data types included with NHibernate will serialize, but if you have any custom data types
(i.e. classes that implement IUserType), they must also be marked with the [Serializable] attribute and,
if necessary, implement ISerializable.

You can configure caching like this:

````c#
		const string configurationFile = "~/nhibernate.database.cfg.xml";

		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure(configurationFile))
			.WithConfiguration.Cached(configurationFile)
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

Where the argument to `WithConfiguration.Cached()` is a params array of dependent file paths. If any of the
files found at those paths has changed, the cache is invalidated. Alternatively, if you are using
FluentNHibernate, you may want to use a different overload which takes a params array of assemblies instead,
like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => Fluently.Configure(new Cfg.Configuration())
				.Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey(sessionFactoryKey)))
				.Mappings(m => m.AutoMappings.Add(AutoMap.Source(new AssemblyTypeSource(Assembly.GetExecutingAssembly()))))
				.BuildConfiguration())
			.WithConfiguration.Cached(Assembly.GetExecutingAssembly())
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

Or you can use one of the other overloads for this method and pass in your own implementation of IConfigurationCache.

###3. Hooking up an initialization callback

Sometimes you want to know when a session factory is initialized so that you can take some action when it occurs,
such as logging or tracing. In order to be notified of when a session factory has been created, you can hook up
an `Action<ISessionFactory>` like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure("~/nhibernate.database.cfg.xml"))
			.WithSessionFactory.OnInitialized(NHibernate.Glimpse.Plugin.RegisterSessionFactory)
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

###4. Lazy/threaded initialization

The act of building a session factory from an NHibernate configuration object can take a significant amount of
time, and if you are registering multiple session factories, then this can add up to a considerable slowdown during
application startup. In order to improve startup performance, you can either defer the initialization of the
session factory until first use, like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure("~/nhibernate.database.cfg.xml"))
			.WithInitialization.Lazy()
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

Or you can specify that initialization should occur on another thread in order to parallelize the
application startup process, like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure("~/nhibernate.database.cfg.xml"))
			.WithInitialization.Threaded()
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

When using threaded initialization, if an error occurs on the background thread it will not be handled and the
behaviour will fall back to lazy initialization upon first use of the session factory. If you know there is a
chance of this happening then you can choose to either ignore the exception like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure("~/nhibernate.database.cfg.xml"))
			.WithInitialization.Threaded(ThreadedInitialization.IgnoreException)
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

Or you can handle the exception by passing in your own `Action<Exception>`.

###5. Hooking up a callback for when a session is opened via the CurrentSessionContext

NHibernate provides the notion of a "current session" for a given session factory so that the same session can
be used by various parts of your application's request processing pipeline. When this has been configured via the
NHibernate Configuration object, you can specify a callback for letting you know when a new session has been
opened, like this:

````c#
		var sessionManager = Configure.NHibernate
			.UsingConfigurationFactory(sessionFactoryKey => new Cfg.Configuration().Configure("~/nhibernate.database.cfg.xml"))
			.WithSession.OnOpened(session => Console.WriteLine(session.Connection.Database))
			.RegisterSessionFactory("Database1")
			.BuildSessionManager();
`````

###6. Session per request for a web application

If your application is a web application, then you can register the provided WebSessionContextModule in order to
implement the session-per-request pattern. At the end of an http request this class takes care of unbinding each
current session context from each corresponding session factory (if you've configured a WebSessionContext as your
ICurrentSessionContext) and then closes the session.

## Usage
Here's how you can use the ISessionManager in your application.

###1. Single session factory configured

If you have only configured one session factory, then you can simply use it like this:

````c#
		var sessionFactory = sessionManager.SessionFactory;
`````

And if you've configured an ICurrentSessionContext, then you can access the current session like this:

````c#
		var currentSession = sessionManager.Session;
`````

###2. Multiple session factories configured

If you have configured more than one session factory, then you can either specify which session factory you
want by providing the session factory key, or (provided that you have decorated a poco class with a
SessionFactoryKeyAttribute) the poco type as a generic argument like this:

````c#
		[SessionFactoryKey("Database1")]
		class Database1Poco { ... }
		...
		var sessionFactory = sessionManager.SessionFactoryFor("Database1");
		sessionFactory = sessionManager.SessionFactoryFor<Database1Poco>();
`````

And if you've configured an ICurrentSessionContext, then you can access the current session like this:

````c#
		[SessionFactoryKey("Database1")]
		class Database1Poco { ... }
		...
		var currentSession = sessionManager.SessionFor("Database1");
		currentSession = sessionManager.SessionFor<Database1Poco>();
`````