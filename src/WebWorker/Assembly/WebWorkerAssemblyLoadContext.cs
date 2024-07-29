using System.Reflection;
using System.Runtime.Loader;

namespace WebWorker.Assembly
{
    public class WebWorkerAssemblyLoadContext : AssemblyLoadContext
    {
        private string? workPath;
        private AssemblyDependencyResolver _resolver;


        public WebWorkerAssemblyLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);

        }

        protected override System.Reflection.Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        ///     Create the Work directory if it does not exist.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void InitWorkDirectory()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (assemblyLocation == null)
            {
                throw new DirectoryNotFoundException("Could not find the assembly location");
            }

            workPath = Path.Combine(Path.GetDirectoryName(assemblyLocation), "Work");

            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
            }
        }
    }
}
