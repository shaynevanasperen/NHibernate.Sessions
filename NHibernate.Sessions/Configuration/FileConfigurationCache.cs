using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NHibernate.Util;
using Quarks;
using Remotion.Linq.Utilities;

namespace NHibernate.Sessions.Configuration
{
	/// <summary>
	/// File cache implementation of IConfigurationCache. Saves and loads a serialized
	/// version of <see cref="Configuration"/> to a temporary file location.
	/// </summary>
	/// <remarks>Serializing a <see cref="Configuration"/> object requires that all components
	/// that make up the Configuration object be Serializable. This includes any custom NHibernate
	/// user types implementing <see cref="NHibernate.UserTypes.IUserType"/>.</remarks>
	public class FileConfigurationCache : IConfigurationCache
	{
		/// <summary>
		/// List of files that the cached configuration is dependent on. If any of these
		/// files are newer than the cache file then the cache file could be out of date.
		/// </summary>
		readonly List<string> _dependentFilePaths = new List<string>();

		/// <summary>
		/// Initializes new instance of the FileConfigurationCache
		/// </summary>
		public FileConfigurationCache() { }

		/// <summary>
		/// Initializes new instance of the FileConfigurationCache using the given dependentFilePaths parameter.
		/// </summary>
		/// <param name="dependentFilePaths">List of files that the cached configuration is dependent upon.</param>
		public FileConfigurationCache(params string[] dependentFilePaths)
			: this()
		{
			if (dependentFilePaths != null)
				appendToDependentFilePaths(dependentFilePaths);
		}

		/// <summary>
		/// Load the <see cref="Configuration"/> object from a cache.
		/// </summary>
		/// <param name="configKey">Key value to provide a unique name to the cached <see cref="Configuration"/>.</param>
		/// <param name="dependentFilePaths">List of files that the cached configuration is dependent upon.</param>
		/// <returns>If an up to date cached object is available, a <see cref="Configuration"/> object, otherwise null.</returns>
		public virtual Cfg.Configuration LoadConfiguration(string configKey, params string[] dependentFilePaths)
		{
			if (string.IsNullOrWhiteSpace(configKey)) throw new ArgumentEmptyException("configKey");

			var cachePath = CachedConfigPath(configKey);
			if (dependentFilePaths != null)
				appendToDependentFilePaths(dependentFilePaths);
			if (IsCachedConfigCurrent(cachePath))
				return FileStore.Load<Cfg.Configuration>(cachePath);

			return null;
		}

		/// <summary>
		/// Save the <see cref="Configuration"/> object to cache to a temporary file.
		/// </summary>
		/// <param name="configKey">Key value to provide a unique name to the cached <see cref="Configuration"/>.</param>
		/// <param name="config">Configuration object to save.</param>
		public virtual void SaveConfiguration(string configKey, Cfg.Configuration config)
		{
			if (string.IsNullOrWhiteSpace(configKey)) throw new ArgumentEmptyException("configKey");
			if (config == null) throw new ArgumentNullException("config");

			var cachePath = CachedConfigPath(configKey);
			FileStore.Save(config, cachePath);
			File.SetLastWriteTime(cachePath, GetLatestDependencyFileWriteTime());
		}

		/// <summary>
		/// Tests whether or not an existing cached configuration file is out of date.
		/// </summary>
		/// <param name="cachePath">Location of the cached</param>
		/// <returns>False if the cached config file is out of date, otherwise true.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the cachePathis null.</exception>
		protected virtual bool IsCachedConfigCurrent(string cachePath)
		{
			return File.Exists(cachePath) && (File.GetLastWriteTime(cachePath) >= GetLatestDependencyFileWriteTime());
		}

		/// <summary>
		/// Returns the latest file write time from the list of dependent file paths.
		/// </summary>
		/// <returns>Latest file write time, or '1/1/1980' if list is empty.</returns>
		protected virtual DateTime GetLatestDependencyFileWriteTime()
		{
			if (_dependentFilePaths == null || _dependentFilePaths.Count == 0)
				return DateTime.Parse("1/1/1980");

			return _dependentFilePaths.Max(n => File.GetLastWriteTime(n));
		}

		/// <summary>
		/// Provide a unique temporary file path/name for the cache file.
		/// </summary>
		/// <param name="configKey">Key value to provide a unique name to the cached <see cref="Configuration"/>.</param>
		/// <returns>Full file path.</returns>
		/// <remarks>The hash value is intended to avoid the file from conflicting with other applications also using this cache feature.</remarks>
		protected virtual string CachedConfigPath(string configKey)
		{
			var fileName = string.Format("{0}-{1}.bin", configKey, Assembly.GetCallingAssembly().CodeBase.GetHashCode());

			return Path.Combine(Path.GetTempPath(), fileName);
		}

		/// <summary>
		/// Append the given list of file paths to the dependentFilePaths list.
		/// </summary>
		/// <param name="paths">List of file paths.</param>
		void appendToDependentFilePaths(IEnumerable<string> paths)
		{
			paths.ForEach(path => _dependentFilePaths.Add(findFile(path)));
		}

		/// <summary>
		/// Tests if the file or assembly name exists either in the application's bin folder or elsewhere.
		/// </summary>
		/// <param name="path">Path or file name to test for existance.</param>
		/// <returns>Full path of the file.</returns>
		/// <remarks>If the path parameter does not end with ".dll" it is appended and tested if the dll file exists.</remarks>
		/// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
		static string findFile(string path)
		{
			if (File.Exists(path))
				return path;

			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var uriPath = Uri.UnescapeDataString(uri.Path);
			var codeLocation = Path.GetDirectoryName(uriPath) ?? string.Empty;

			var codePath = Path.Combine(codeLocation, path);
			if (File.Exists(codePath))
				return codePath;

			var dllPath = (path.IndexOf(".dll", StringComparison.Ordinal) == -1) ? path.Trim() + ".dll" : path.Trim();
			if (File.Exists(dllPath))
				return dllPath;

			var codeDllPath = Path.Combine(codeLocation, dllPath);
			if (File.Exists(codeDllPath))
				return codeDllPath;

			var exePath = (path.IndexOf(".exe", StringComparison.Ordinal) == -1) ? path.Trim() + ".exe" : path.Trim();
			if (File.Exists(exePath))
				return exePath;

			var codeExePath = Path.Combine(codeLocation, exePath);
			if (File.Exists(codeExePath))
				return codeExePath;

			throw new FileNotFoundException("Unable to find file.", path);
		}
	}
}
