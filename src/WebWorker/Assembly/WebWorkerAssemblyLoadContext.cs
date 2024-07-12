using System.Reflection;
using System.Runtime.Loader;

namespace WebWorker.Assembly
{
    public class WebWorkerAssemblyLoadContext : AssemblyLoadContext
    {
        private static string? workPath;

        protected override System.Reflection.Assembly? Load(AssemblyName assemblyName)
        {
            // Attempt to load the requested assembly from the default context
            var assembly = Default.LoadFromAssemblyName(assemblyName);

            if (assembly != null)
            {
                return assembly;
            }

            // If not found, attempt to load it from other sources if needed
            string assemblyPath = $"/Work/{assemblyName.Name}/{assemblyName.Name}.dll";

            return File.Exists(assemblyPath) ? LoadFromAssemblyPath(assemblyPath) : null;
        }

        public void LoadAssemblyAndDependencies(string assemblyPath)
        {
            var mainAssembly = LoadFromAssemblyPath(assemblyPath);

            ResolveDependencies(mainAssembly);
        }

        private static void ResolveDependencies(System.Reflection.Assembly mainAssembly)
        {
            var assembliesToLoad = new Queue<AssemblyName>(mainAssembly.GetReferencedAssemblies());
            var loadedAssemblies = new HashSet<string>();

            while (assembliesToLoad.Count > 0)
            {
                var assemblyName = assembliesToLoad.Dequeue();

                if (!loadedAssemblies.Contains(assemblyName.FullName))
                {
                    var loadedAssembly = Default.LoadFromAssemblyName(assemblyName);
                    if (loadedAssembly != null)
                    {
                        loadedAssemblies.Add(assemblyName.FullName);

                        foreach (var referencedAssembly in loadedAssembly.GetReferencedAssemblies())
                        {
                            assembliesToLoad.Enqueue(referencedAssembly);
                        }
                    }
                }
            }
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
