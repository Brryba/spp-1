using System.Reflection;
using System.Runtime.Loader;
using Tracer.Core; 
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization
{
    public class PluginManager
    {
        private readonly List<ITraceResultSerializer> _serializers;

        public PluginManager()
        {
            _serializers = new List<ITraceResultSerializer>();
        }

        public void LoadPlugins()
        {
            string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            var pluginFiles = Directory.GetFiles(executionPath, "Tracer.Serialization.*.dll");

            foreach (var file in pluginFiles)
            {
                try
                {
                    if (file.Contains("Abstractions") || file.EndsWith("Tracer.Serialization.dll")) 
                        continue;
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    
                    var types = assembly.GetTypes()
                        .Where(t => typeof(ITraceResultSerializer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        var serializer = (ITraceResultSerializer)Activator.CreateInstance(type);
                        _serializers.Add(serializer);
                        Console.WriteLine($"[Loader] Loaded plugin: {serializer.Format.ToUpper()} from {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Loader] Error loading {file}: {ex.Message}");
                }
            }
        }

        public void SaveResult(TraceResult result, string directoryPath, string fileNameWithoutExtension)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            foreach (var serializer in _serializers)
            {
                string fileName = $"{fileNameWithoutExtension}.{serializer.Format}";
                
                string fullPath = Path.Combine(directoryPath, fileName);
                
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    serializer.Serialize(result, fileStream);
                }
                
                Console.WriteLine($"Saved trace result to: {Path.GetFullPath(fullPath)}");
            }
        }
    }
}