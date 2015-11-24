namespace NHibernate.Sessions.Configuration
{
	/// <summary>
	/// Interface for providing caching capability for an <see cref="Configuration"/> object.
	/// </summary>
	public interface IConfigurationCache
	{
		/// <summary>
		/// Load the <see cref="Configuration"/> object from a cache.
		/// </summary>
		/// <param name="configKey">Key value to provide a unique name to the cached <see cref="Configuration"/>.</param>
		/// <param name="dependentFilePaths">List of files that the cached configuration is dependent upon.</param>
		/// <returns>If an up to date cached object is available, a <see cref="Configuration"/> object, otherwise null.</returns>
		Cfg.Configuration LoadConfiguration(string configKey, params string[] dependentFilePaths);

		/// <summary>
		/// Save the <see cref="Configuration"/> object to a cache.
		/// </summary>
		/// <param name="configKey">Key value to provide a unique name to the cached <see cref="Configuration"/>.</param>
		/// <param name="config">Configuration object to save.</param>
		void SaveConfiguration(string configKey, Cfg.Configuration config);
	}
}