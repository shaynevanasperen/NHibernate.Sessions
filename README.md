<img src="NHibernate.Sessions.png" align="right" />
[![Build status](https://ci.appveyor.com/api/projects/status/5g0gjfqs2sd48tja?svg=true)](https://ci.appveyor.com/project/shaynevanasperen/nhibernate-sessions)
[![NuGet](https://buildstats.info/nuget/NHibernate.Sessions)](https://www.nuget.org/packages/NHibernate.Sessions)
NHibernate.Sessions
===================

A helpful library for configuring and managing NHibernate session factories and sessions. Provides a fluent
interface for configuring either a single session factory, or multiple session factories, with support for:

   * NHibernate configuration caching for faster startup
   * Lazy session factory initialization (deferred until first use)
   * Background session factory initialization (parallelized in startup)
   * Lazy opening of sessions for current session context (deferred until first use)
   * Automatic scoped current session context handling (unbinding and disposing of session on scope completion)
   * Session factory initialized callbacks
   * Session opened callbacks
   * Strongly typed contexts (refer to particular named session factory)

Session factories and sessions are exposed via six main classes:

   * `LazySessionFactory` - for accessing the session factory in a single session factory scenario
     * *implements `ILazySessionFactory`*
   * `LazySession` - for scoping and accessing the current session in a single session factory scenario
     * *implements `ILazySession`, `ILazySessionScoper`*
   * `LazySessionFactories` - for accessing a session factory by key in a multiple session factories scenario
     * *implements `ILazySessionFactories`*
   * `LazySessions` - for scoping and accessing the current session by factory key in a multiple session factories scenario
     * *implements `ILazySessions`, `ILazySessionsScoper`*
   * `SessionFactoryContext` - an abstract base class for defining access to a particular session factory
     * *implements `ISessionFactoryContext`*
   * `SessionContext` - an abstract base class for defining access to sessions associated with a particular session factory
     * *implements `ISessionContext`*

## Configuration
The fluent interface for producing instances of these classes begins with either `ConfigureNHibernate.ForSingleSessionFactory(...)`
or `ConfigureNHibernate.ForMultipleSessionFactories(...)`.

You can then use method chaining to call any of the following methods:

   * `WithCachedConfiguration(...)`
   * `WithoutCachedConfiguration()`
   * `WithEagerInitialization()`
   * `WithLazyInitialization()`
   * `WithBackgroundInitialization(...)`
   * `WithInitializedCallback(...)`
   * `WithSessionOpenedCallback(...)`
    
Then, if configuring for a single session factory, you can call either `CreateLazySessionFactory()` or
`CreateLazySession()`, otherwise if configuring for multiple session factories, you can call `RegisterSessionFactory(...)`.
When configuring for multiple session factories, once you have called `RegisterSessionFactory(...)`, you
can then call `RegisterSessionFactory(...)` (to register another session factory using the same options),
`CreateLazySessionFactories()`, or `CreateLazySessions()`. Between each successive call to `RegisterSessionFactory(...)`,
you can call any of the above list of methods in order to alter the options used for the subsequent call
to `RegisterSessionFactory(...)`.


Here are some configuration examples:

```c#
LazySessionFactory lazySessionFactory = ConfigureNHibernate
    .ForSingleSessionFactory(getConfiguration)
	.WithCachedConfiguration(Assembly.GetExecutingAssembly())
    .WithEagerInitialization()
    .WithInitializedCallback(x => Console.WriteLine("{0} initialized", x.GetType().Name))
    .CreateLazySessionFactory();

LazySession lazySession = ConfigureNHibernate
    .ForSingleSessionFactory(() => getConfiguration())
	.WithCachedConfiguration()
    .WithLazyInitialization()
    .WithInitializedCallback(x => Console.WriteLine("{0} initialized", x.GetType().Name))
    .WithSessionOpenedCallback(x => Console.WriteLine("{0} opened", x.GetType().Name))
    .CreateLazySession();

LazySessionFactories lazySessionFactories = ConfigureNHibernate
    .ForMultipleSessionFactories(getConfiguration) // getConfiguration is a Func<string, Configuration>
	.WithCachedConfiguration(configurationCache, Assembly.GetExecutingAssembly())
    .WithBackgroundInitialization()
    .RegisterSessionFactory("TodoItems")
    .RegisterSessionFactory("Users") // same options used for second session factory, but using a different key which gets passed to the provided Func<string, Configuration>
    .CreateLazySessionFactories();

LazySessions lazySessions = ConfigureNHibernate
    .ForMultipleSessionFactories(key => getConfiguration(key))
    .WithCachedConfiguration("C:\\Application\\bin\\Application.dll")
    .WithBackgroundInitialization(BackgroundInitialization.IgnoreException) // falls back to lazy on exception
    .WithInitializedCallback(x => Console.WriteLine("{0} initialized", x.GetType().Name))
    .RegisterSessionFactory<TodoItems>() // using a type name as a key
    .RegisterSessionFactory<Users>() // same options used for second session factory, but using a different key which gets passed to the provided Func<string, Configuration>
    .CreateLazySessions();

var multiSessionFactoryConfigurer = ConfigureNHibernate
    .ForMultipleSessionFactories(getConfiguration) // getConfiguration is a Func<string, Configuration>
    .WithCachedConfiguration(typeof(TodoItem).Assembly)
    .WithBackgroundInitialization(handleException) // handleException is an Action<Exception>
    .WithInitializedCallback(todoItemsSessionFactoryInitialized) // todoItemsSessionFactoryInitialized is an Action<ISessionFactory>
    .WithSessionOpenedCallback(todoItemsSessionOpened) // todoItemsSessionOpened is an Action<ISession>
    .RegisterSessionFactory<TodoItems>() // using a type name as a key
    .WithoutCachedConfiguration() // different option for the second session factory
    .WithLazyInitialization() // different option for the second session factory
    .WithInitializedCallback(usersSessionFactoryInitialized) // usersSessionFactoryInitialized is an Action<ISessionFactory>
    .WithSessionOpenedCallback(usersSessionOpened) // usersSessionOpened is an Action<ISession>
    .RegisterSessionFactory<Users>(); // using a type name as a key

// Use the same configurer object to create both LazySessionFactories and LazySessions objects
lazySessionFactories = multiSessionFactoryConfigurer.CreateLazySessionFactories();
lazySessions = multiSessionFactoryConfigurer.CreateLazySessions();

```

### NHibernate configuration caching

During an application's startup NHibernate can take significant time when configuring and validating its
mapping to a database. Caching the NHibernate configuration data can reduce initial startup time by storing
the configuration to a file and avoiding the validation checks that run when a configuration is created
from scratch.

The default cache implementation is a file cache which serializes the configuration to a file.
For this to work, all objects contained within the NHibernate configuration must be serializable. All of
the default data types included with NHibernate will serialize, but if you have any custom data types
(i.e. classes that implement `IUserType`), they must also be marked with the `[Serializable]` attribute and,
if necessary, implement `ISerializable`.

You can specify configuration caching by calling `WithCachedConfiguration(...)`, where the argument is
a params array of dependent file paths. If any of the files found at those paths has changed, the cache is
invalidated. Alternatively, if you are using FluentNHibernate, you may want to use a different overload where
the argument is a params array of assemblies instead. Or you can use one of the other overloads for this
method and pass in your own implementation of `IConfigurationCache`.

### Lazy/Background initialization

The act of building a session factory from an NHibernate configuration object can take a significant amount of
time, and if you are registering multiple session factories, then this can add up to a considerable slowdown during
application startup. In order to improve startup performance, you can either defer the initialization of the
session factory until first use by calling `WithLazyInitialization()`, or you can specify that each session
factory is initialized in a separate background task by calling `WithBackgroundInitialization(...)`. Background
initialization is achieved by using `Task.Factory.StartNew(..., TaskCreationOptions.LongRunning)`.

When using background initialization, if an error occurs on the background thread it will not be handled and the
behaviour will fall back to lazy initialization upon first use of the session factory. If you know there is a
chance of this happening then you can choose to either ignore the exception by using the supplied no-op action
`BackgroundInitialization.IgnoreException` as the first argument to `WithBackgroundInitialization(...)`, or you
can handle the exception by passing in your own `Action<Exception>`.

### Initialization callback

Sometimes you want to know when a session factory is initialized so that you can take some action when it occurs,
such as logging, tracing or integration with third party libraries. In order to be notified of when a session
factory has been created, you can specify an `Action<ISessionFactory>` by calling `WithInitializedCallback(...)`.

### Session opened callback

NHibernate provides the notion of a "current session" for a given session factory so that the same session can
be used by various parts of your application's request processing pipeline. When this has been configured via the
NHibernate `Configuration` object and you're using either `ILazySession` or `ILazySessions`, you can specify a
callback for letting you know when a new session has been opened by calling `WithSessionOpenedCallback(...)` where
the argument is an `Action<ISession>`.

## Usage
The following six main interfaces are designed to be used as singletons:

   * `ILazySessionFactory` provides lazy access to the session factory via its `Value` property.
   * `ILazySessionFactories` represents a collection of `ILazySessionFactory` instances and provides lazy access
    to a particular session factory by key via its `ValueFor(...)` methods.
   * `ILazySession` provides lazy access to the current session (as defined by the underlying `ISessionFactory`'s
    configured current session context) via its `Current` property. If you're using a custom `ICurrentSessionContext`
    which doesn't derive from either `CurrentSessionContext` or `ThreadLocalSessionContext`, you'll need to specify a
    custom `ICurrentSessionContextBinder` via the `CurrentSessionContextBinder(...)` extension methods for `Configuration`.
   * `ILazySessions` represents a collection of `ILazySession` instances and provides lazy access to a particular
    current session by factory key via its `CurrentFor(...)` methods.
   * `ILazySessionScoper` manages the scoping of current session context by unbinding and disposing the current
    session when the scope completes. Scope completion can be performed via the `UnbindAndDisposeCurrent()` method,
    or via the `Dispose` method of the `ILazySessionScope` object returned from calling the `Scope()` method.
   * `ILazySessionsScoper` represents a collection of `ILazySessionScoper` instances and manages the scoping of
    current session context by factory key via its `ScopeFor(...)` methods. Individual scopes obtained from calling
    `ScopeFor(...)` can be completed by calling their `Dispose` methods. Its `Scope()` method can be used to create
    a global scope for all current sessions, and its `UnbindAndDisposeCurrent()` method can be used to complete the
    global scope.

The following two main interfaces are designed to be used transiently:

   * `ISessionFactoryContext` provides access to a specific session factory via its `SessionFactory` property.
   * `ISessionContext` provides access to sessions associated with a specific session factory via its `Session` property.

*Note: Don't use `ThreadStaticSessionContext` in multiple session factory scenarios, as it isn't compatible.*

When using NHibernate current session context, it's important to unbind and dispose the current session when
the logical scope completes (as defined by the lifecycle of "current"). You would typically unbind and dispose
the session at the end of a business transaction. In a web application, this is commonly implemented using the
session-per-request pattern. To enable this pattern with `ILazySession` or `ILazySessions`, you can call
`UnbindAndDisposeCurrent()` when the scope completes (during the `EndRequest` in a web application). Alternatively,
you can obtain a new `ILazySessionScope` or `ILazySessionsScope` when the scope begins (during the `BeginRequest`
in a web application) via the `Scope()` or `ScopeFor(...)` methods and then dipose it when the scope completes.

Here are some usage examples:

```c#
    ISessionFactory sessionFactory = _lazySessionFactory.Value;

    sessionFactory = _lazySessionFactories.ValueFor("Users");
    sessionFactory = _lazySessionFactories.ValueFor<Users>(); // using a type name as a key

    ISession session = _lazySession.Current;

    session = _lazySessions.CurrentFor("Users");
    session = _lazySessions.CurrentFor<Users>(); // using a type name as a key

    _lazySessionScoper.UnbindAndDisposeCurrent(); // when the scope completes
    _lazySessionsScoper.UnbindAndDisposeCurrent(); // when the scope completes

    using (ILazySessionScope scope = _lazySessionScoper.Scope()) // when the scope begins
    {
        session = scope.Current;
    } // when the scope completes, Dispose() is called which in turn calls UnbindAndDisposeCurrent()

    using (ILazySessionsScope scope = _lazySessionsScoper.Scope())
    {
        session = scope.CurrentFor("Users");
        session = scope.CurrentFor<Users>(); // using a type name as a key
    }

    using (ILazySessionScope scope = _lazySessionsScoper.ScopeFor("Users"))
    {
        session = scope.Current;
    }

    using (ILazySessionScope scope = _lazySessionsScoper.ScopeFor<Users>())) // using a type name as a key
    {
        session = scope.Current;
    }
```

### Strongly typed contexts

In a multiple session factories scenario it can be useful to create a separate context class for each session
factory being used so that you can refer to it in a strongly typed way. You can easily define your own named
contexts by inheriting from either `SessionFactoryContext` or `SessionContext`, as shown in the following examples:

```c#
    class UsersFactoryContext : SessionFactoryContext
	{
		public UsersFactoryContext(Func<ISessionFactory> sessionFactoryFunc) : base(sessionFactoryFunc) { }
		public UsersFactoryContext(ILazySessionFactory lazySessionFactory) : base(lazySessionFactory) { }
	}

    class UsersFactoryComponent
    {
        readonly UsersFactoryContext _usersFactoryContext;

        public UsersFactoryComponent(UsersFactoryContext usersFactoryContext)
        {
            _usersFactoryContext = usersFactoryContext;
            var sessionFactory = _usersFactoryContext.SessionFactory;
        }
    }

    var usersFactoryContext = new UsersFactoryContext(_lazySessionFactory); // create a context wrapping an ILazySessionFactory

    usersFactoryContext = new UsersFactoryContext(() => _lazySessionFactories.ValueFor("Users")); // create a context wrapping a Func<ISessionFactory>
    usersFactoryContext = new UsersFactoryContext(() => _lazySessionFactories.ValueFor<Users>()); // using a type name as a key

    class UsersContext : SessionContext
	{
		public UsersContext(Func<ISession> sessionFunc) : base(sessionFunc) { }
		public UsersContext(ILazySessionScope lazySessionScope) : base(lazySessionScope) { }
	}

    class UsersComponent
    {
        readonly UsersContext _usersContext;

        public UsersComponent(UsersContext usersContext)
        {
            _usersContext = usersContext;
            var session = _usersContext.Session;
        }
    }

    var usersContext = new UsersContext(_lazySessionsScope.ScopeFor("Users")); // create a context wrapping an ILazySessionScope
    usersContext = new UsersContext(_lazySessionsScope.ScopeFor<Users>()); // using a type name as a key

    usersContext = new UsersContext(() => _lazySessionFactories.ValueFor("Users").OpenSession()); // create a context wrapping a Func<ISession>
    usersContext = new UsersContext(() => _lazySessionFactories.ValueFor<Users>().OpenSession()); // using a type name as a key
```

*See further examples in the accompanying demo application*