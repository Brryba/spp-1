using System.Collections.Concurrent;
using System.Diagnostics;

namespace Tracer.Core
{
    public class Tracer : ITracer
    {
        private readonly ConcurrentDictionary<int, ThreadTracer> _threadTracers;

        public Tracer()
        {
            _threadTracers = new ConcurrentDictionary<int, ThreadTracer>();
        }

        public void StartTrace()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            
            var threadTracer = _threadTracers.GetOrAdd(threadId, _ => new ThreadTracer(threadId));
            
            threadTracer.StartTrace();
        }

        public void StopTrace()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            
            if (_threadTracers.TryGetValue(threadId, out var threadTracer))
            {
                threadTracer.StopTrace();
            }
        }

        public TraceResult GetTraceResult()
        {
            var threadResults = new List<ThreadTraceResult>();
            
            foreach (var threadTracer in _threadTracers.Values)
            {
                threadResults.Add(threadTracer.GetResult());
            }

            return new TraceResult(threadResults);
        }
    }

    internal class ThreadTracer
    {
        private readonly int _threadId;
        private readonly Stack<MethodInfoTemp> _methodStack; 
        private readonly List<MethodTraceResult> _rootMethods;

        public ThreadTracer(int threadId)
        {
            _threadId = threadId;
            _methodStack = new Stack<MethodInfoTemp>();
            _rootMethods = new List<MethodTraceResult>();
        }

        public void StartTrace()
        {
            var methodInfo = new MethodInfoTemp();
            methodInfo.Stopwatch.Start();
            
            _methodStack.Push(methodInfo);
        }

        public void StopTrace()
        {
            if (_methodStack.Count == 0) return;

            var finishedMethod = _methodStack.Pop();
            finishedMethod.Stopwatch.Stop();
            
            
            var stackTrace = new StackTrace(1); 
            var frame = stackTrace.GetFrame(0);
            var methodBase = frame?.GetMethod();

            string methodName = methodBase?.Name ?? "Unknown";
            string className = methodBase?.DeclaringType?.Name ?? "Unknown";

            var result = new MethodTraceResult(
                methodName, 
                className, 
                finishedMethod.Stopwatch.ElapsedMilliseconds, 
                finishedMethod.Children
            );

            if (_methodStack.Count > 0)
            {
                var parent = _methodStack.Peek();
                parent.Children.Add(result);
            }
            else
            {
                _rootMethods.Add(result);
            }
        }

        public ThreadTraceResult GetResult()
        {
            long totalTime = _rootMethods.Sum(m => m.Time);
            return new ThreadTraceResult(_threadId, totalTime, new List<MethodTraceResult>(_rootMethods));
        }

        private class MethodInfoTemp
        {
            public Stopwatch Stopwatch { get; } = new Stopwatch();
            public List<MethodTraceResult> Children { get; } = new List<MethodTraceResult>();
        }
    }
}