using Tracer.Core;
using Tracer.Serialization;

namespace Tracer.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Start Tracing (Multi-threaded) ---");

            ITracer tracer = new Tracer.Core.Tracer();

            var heavyWorker = new HeavyWorker(tracer);
            var recursiveWorker = new RecursiveWorker(tracer);

            Thread thread1 = new Thread(heavyWorker.MethodA);
            thread1.Name = "Heavy-Thread";

            Thread thread2 = new Thread(() => recursiveWorker.Recurse(3)); 
            thread2.Name = "Recursive-Thread";

            thread1.Start();
            thread2.Start();

            tracer.StartTrace();
            heavyWorker.MethodB();
            tracer.StopTrace();

            thread1.Join();
            thread2.Join();

            TraceResult result = tracer.GetTraceResult();

            Console.WriteLine("\n--- Saving Results ---");
            
            var pluginManager = new PluginManager();
            pluginManager.LoadPlugins();
            
            pluginManager.SaveResult(result, "Result", "result");
            
            Console.WriteLine("Done. Check the 'Result' folder.");
        }
    }

    public class HeavyWorker
    {
        private readonly ITracer _tracer;
        private readonly SubWorker _subWorker;

        public HeavyWorker(ITracer tracer)
        {
            _tracer = tracer;
            _subWorker = new SubWorker(tracer);
        }

        public void MethodA()
        {
            _tracer.StartTrace();
            Thread.Sleep(50); 
            
            MethodB(); 
            
            _tracer.StopTrace();
        }

        public void MethodB()
        {
            _tracer.StartTrace();
            Thread.Sleep(30);
            
            _subWorker.OtherMethod(); 
            
            _tracer.StopTrace();
        }
    }

    public class SubWorker
    {
        private readonly ITracer _tracer;

        public SubWorker(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void OtherMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(20);
            Console.WriteLine("Deep work done.");
            _tracer.StopTrace();
        }
    }

    public class RecursiveWorker
    {
        private readonly ITracer _tracer;

        public RecursiveWorker(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void Recurse(int depth)
        {
            _tracer.StartTrace();
            
            Thread.Sleep(10);
            
            if (depth > 0)
            {
                Recurse(depth - 1);
            }

            _tracer.StopTrace();
        }
    }
}