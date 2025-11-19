using System.Reflection;
using System.Runtime.Loader;

namespace Infernity.Framework.Plugins.Isolation
{
    /// <summary>
    /// Represents the method that will handle the <see cref="AssemblyLoader.Reloaded" /> event.
    /// </summary>
    /// <param name="sender">The object sending the event</param>
    /// <param name="eventArgs">Data about the event.</param>
    public delegate void AssemblyReloadedEventHandler(object sender, AssemblyReloadedEventArgs eventArgs);

    /// <summary>
    /// Provides data for the <see cref="AssemblyLoader.Reloaded" /> event.
    /// </summary>
    public class AssemblyReloadedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes <see cref="AssemblyReloadedEventArgs" />.
        /// </summary>
        /// <param name="loader"></param>
        public AssemblyReloadedEventArgs(AssemblyLoader loader)
        {
            Loader = loader;
        }

        /// <summary>
        /// The plugin loader
        /// </summary>
        public AssemblyLoader Loader { get; }
    }
    
    /// <summary>
    /// This loader attempts to load binaries for execution (both managed assemblies and native libraries)
    /// in the same way that .NET would if they were originally part of the .NET application.
    /// <para>
    /// This loader reads configuration files produced by .NET  (.deps.json and runtimeconfig.json)
    /// as well as a custom file (*.config files). These files describe a list of .dlls and a set of dependencies.
    /// The loader searches the plugin path, as well as any additionally specified paths, for binaries
    /// which satisfy the plugin's requirements.
    /// </para>
    /// </summary>
    public class AssemblyLoader : IDisposable
    {
        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="isUnloadable">Enable unloading the plugin from memory.</param>
        /// <param name="sharedTypes">
        /// <para>
        /// A list of types which should be shared between the host and the plugin.
        /// </para>
        /// <para>
        /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md">
        /// https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md
        /// </seealso>
        /// </para>
        /// </param>
        /// <returns>A loader.</returns>
        public static AssemblyLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, _ => { });

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="isUnloadable">Enable unloading the plugin from memory.</param>
        /// <param name="sharedTypes">
        /// <para>
        /// A list of types which should be shared between the host and the plugin.
        /// </para>
        /// <para>
        /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md">
        /// https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md
        /// </seealso>
        /// </para>
        /// </param>
        /// <param name="configure">A function which can be used to configure advanced options for the plugin loader.</param>
        /// <returns>A loader.</returns>
        public static AssemblyLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes, Action<AssemblyLoaderConfiguration> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                    sharedTypes,
                    config =>
                    {
                        config.IsUnloadable = isUnloadable;
                        configure(config);
                    });
        }

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="sharedTypes">
        /// <para>
        /// A list of types which should be shared between the host and the plugin.
        /// </para>
        /// <para>
        /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md">
        /// https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md
        /// </seealso>
        /// </para>
        /// </param>
        /// <returns>A loader.</returns>
        public static AssemblyLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="sharedTypes">
        /// <para>
        /// A list of types which should be shared between the host and the plugin.
        /// </para>
        /// <para>
        /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md">
        /// https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md
        /// </seealso>
        /// </para>
        /// </param>
        /// <param name="configure">A function which can be used to configure advanced options for the plugin loader.</param>
        /// <returns>A loader.</returns>
        public static AssemblyLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes, Action<AssemblyLoaderConfiguration> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                    config =>
                    {
                        if (sharedTypes != null)
                        {
                            var uniqueAssemblies = new HashSet<Assembly>();
                            foreach (var type in sharedTypes)
                            {
                                uniqueAssemblies.Add(type.Assembly);
                            }

                            foreach (var assembly in uniqueAssemblies)
                            {
                                config.SharedAssemblies.Add(assembly.GetName());
                            }
                        }
                        configure(config);
                    });
        }

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <returns>A loader.</returns>
        public static AssemblyLoader CreateFromAssemblyFile(string assemblyFile)
            => CreateFromAssemblyFile(assemblyFile, _ => { });

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="configure">A function which can be used to configure advanced options for the plugin loader.</param>
        /// <returns>A loader.</returns>
        public static AssemblyLoader CreateFromAssemblyFile(string assemblyFile, Action<AssemblyLoaderConfiguration> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var config = new AssemblyLoaderConfiguration(assemblyFile);
            configure(config);
            return new AssemblyLoader(config);
        }

        private readonly AssemblyLoaderConfiguration _configuration;
        private ManagedLoadContext _context;
        private readonly AssemblyLoadContextBuilder _contextBuilder;
        private volatile bool _disposed;

        /// <summary>
        /// Initialize an instance of <see cref="AssemblyLoader" />
        /// </summary>
        /// <param name="configuration">The configuration for the plugin.</param>
        private AssemblyLoader(AssemblyLoaderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _contextBuilder = CreateLoadContextBuilder(configuration);
            _context = (ManagedLoadContext)_contextBuilder.Build();
        }

        /// <summary>
        /// True when this plugin is capable of being unloaded.
        /// </summary>
        public bool IsUnloadable => _context.IsCollectible;
        
        public event AssemblyReloadedEventHandler? Reloaded;

        /// <summary>
        /// The unloads and reloads the plugin assemblies.
        /// This method throws if <see cref="IsUnloadable" /> is <c>false</c>.
        /// </summary>
        public void Reload()
        {
            EnsureNotDisposed();

            if (!IsUnloadable)
            {
                throw new InvalidOperationException("Reload cannot be used because IsUnloadable is false");
            }

            _context.Unload();
            _context = (ManagedLoadContext)_contextBuilder.Build();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Reloaded?.Invoke(this, new AssemblyReloadedEventArgs(this));
        }

        /// <summary>
        /// Gets the assembly load context which represents the runtime's concept of a scope for assembly loading.
        /// </summary>
        public AssemblyLoadContext LoadContext => _context;

        /// <summary>
        /// Load the main assembly for the plugin.
        /// </summary>
        public Assembly LoadDefaultAssembly()
        {
            EnsureNotDisposed();
            return _context.LoadAssemblyFromFilePath(_configuration.MainAssemblyPath);
        }

        public Assembly LoadDefaultAssemblyForReflectionOnly()
        {
            EnsureNotDisposed();
            return _context.LoadAssemblyFromFilePath(_configuration.MainAssemblyPath);
        }

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(AssemblyName assemblyName)
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyName(assemblyName);
        }

        /// <summary>
        /// Load an assembly from path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssemblyFromPath(string assemblyPath)
            => _context.LoadAssemblyFromFilePath(assemblyPath);

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(string assemblyName)
        {
            EnsureNotDisposed();
            return LoadAssembly(new AssemblyName(assemblyName));
        }

        /// <summary>
        /// Sets the scope used by some System.Reflection APIs which might trigger assembly loading.
        /// <para>
        /// See https://github.com/dotnet/coreclr/blob/v3.0.0/Documentation/design-docs/AssemblyLoadContext.ContextualReflection.md for more details.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public AssemblyLoadContext.ContextualReflectionScope EnterContextualReflection()
            => _context.EnterContextualReflection();

        /// <summary>
        /// Disposes the plugin loader. This only does something if <see cref="IsUnloadable" /> is true.
        /// When true, this will unload assemblies which which were loaded during the lifetime
        /// of the plugin.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_context.IsCollectible)
            {
                _context.Unload();
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssemblyLoader));
            }
        }

        private static AssemblyLoadContextBuilder CreateLoadContextBuilder(AssemblyLoaderConfiguration configuration)
        {
            var builder = new AssemblyLoadContextBuilder();

            builder.SetMainAssemblyPath(configuration.MainAssemblyPath);
            builder.SetDefaultContext(configuration.DefaultContext);

            foreach (var ext in configuration.PrivateAssemblies)
            {
                builder.PreferLoadContextAssembly(ext);
            }

            if (configuration.PreferSharedTypes)
            {
                builder.PreferDefaultLoadContext(true);
            }

            if (configuration.IsUnloadable)
            {
                builder.EnableUnloading();
            }

            if (configuration.LoadInMemory)
            {
                builder.PreloadAssembliesIntoMemory();
                builder.ShadowCopyNativeLibraries();
            }

            builder.IsLazyLoaded(configuration.IsLazyLoaded);
            foreach (var assemblyName in configuration.SharedAssemblies)
            {
                builder.PreferDefaultLoadContextAssembly(assemblyName);
            }

            var baseDir = Path.GetDirectoryName(configuration.MainAssemblyPath);
            var assemblyFileName = Path.GetFileNameWithoutExtension(configuration.MainAssemblyPath);

            if (baseDir == null)
            {
                throw new InvalidOperationException("Could not determine which directory to watch. "
                + "Please set MainAssemblyPath to an absolute path so its parent directory can be discovered.");
            }

            var pluginRuntimeConfigFile = Path.Combine(baseDir, assemblyFileName + ".runtimeconfig.json");

            builder.TryAddAdditionalProbingPathFromRuntimeConfig(pluginRuntimeConfigFile, includeDevConfig: true, out _);

            // Always include runtimeconfig.json from the host app.
            // in some cases, like `dotnet test`, the entry assembly does not actually match with the
            // runtime config file which is why we search for all files matching this extensions.
            foreach (var runtimeconfig in Directory.GetFiles(AppContext.BaseDirectory, "*.runtimeconfig.json"))
            {
                builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeconfig, includeDevConfig: true, out _);
            }

            return builder;
        }
    }
}
