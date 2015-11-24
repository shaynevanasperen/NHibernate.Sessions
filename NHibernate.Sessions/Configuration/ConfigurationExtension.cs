namespace NHibernate.Sessions.Configuration
{
	public static class ConfigurationExtension
	{
		public const string CurrentSessionContextBinderClass = "current_session_context_binder_class";

		public static Cfg.Configuration CurrentSessionContextBinder(this Cfg.Configuration configuration, string currentSessionContextBinderClass)
		{
			return configuration.SetProperty(CurrentSessionContextBinderClass, currentSessionContextBinderClass);
		}

		public static Cfg.Configuration CurrentSessionContextBinder<TSessionContextBinder>(this Cfg.Configuration configuration) where TSessionContextBinder : ICurrentSessionContextBinder
		{
			return configuration.CurrentSessionContextBinder(typeof(TSessionContextBinder).AssemblyQualifiedName);
		}
	}
}
