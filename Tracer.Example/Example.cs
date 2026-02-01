using Tracer.Core;

namespace Tracer.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            ITracer tracer = new Core.Tracer();

            var foo = new Foo(tracer);

            for (int i = 0; i < 10; i++)
            {
                Thread thread = new Thread(() => 
                {
                    foo.MyMethod(); 
                });
                thread.Start();
            }
            
            Thread.Sleep(1000);

            TraceResult result = tracer.GetTraceResult();

            PrintResultToConsole(result);

            Console.ReadLine();
        }

        static void PrintResultToConsole(TraceResult result)
        {
            foreach (var thread in result.Threads)
            {
                Console.WriteLine($"Thread ID: {thread.ThreadId}, Total Time: {thread.Time}ms");
                foreach (var method in thread.Methods)
                {
                    PrintMethod(method, 1);
                }
            }
        }

        static void PrintMethod(MethodTraceResult method, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            Console.WriteLine($"{indent}Method: {method.MethodName} ({method.ClassName}), Time: {method.Time}ms");

            foreach (var child in method.Methods)
            {
                PrintMethod(child, indentLevel + 1);
            }
        }
    }

    public class Foo
    {
        private readonly ITracer _tracer;
        private readonly Bar _bar;

        public Foo(ITracer tracer)
        {
            _tracer = tracer;
            _bar = new Bar(_tracer);
        }

        public void MyMethod()
        {
            _tracer.StartTrace();
            
            Thread.Sleep(100); 
            
            _bar.InnerMethod();

            _tracer.StopTrace();
        }
    }

    public class Bar
    {
        private readonly ITracer _tracer;

        public Bar(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void InnerMethod()
        {
            _tracer.StartTrace();
            
            Thread.Sleep(50);
            
            _tracer.StopTrace();
        }
    }
}