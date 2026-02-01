using System.Collections.Concurrent;
using System.Diagnostics;

namespace Tracer.Core
{
    public class Tracer : ITracer
    {
        private readonly ConcurrentDictionary<int, ThreadTracer> _threadTracers = new();

        public void StartTrace()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            _threadTracers.GetOrAdd(threadId, new ThreadTracer(threadId)).StartTrace();
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
            return new TraceResult(_threadTracers.Values.Select(t => t.GetResult()).ToList());
        }
    }

    internal class ThreadTracer
    {
        private readonly int _threadId;
        private readonly Stack<MethodInfo> _methodStack = new();
        private readonly List<MethodTraceResult> _rootMethods = new();

        public ThreadTracer(int threadId) => _threadId = threadId;

        public void StartTrace()
        {
            _methodStack.Push(new MethodInfo());
        }

        public void StopTrace()
        {
            if (_methodStack.Count == 0) return;

            var method = _methodStack.Pop();
            method.Stopwatch.Stop();

            var frame = new StackTrace(false).GetFrame(2);
            var methodBase = frame?.GetMethod();

            var result = new MethodTraceResult(
                methodBase?.Name ?? "Unknown",
                methodBase?.DeclaringType?.Name ?? "Unknown",
                method.Stopwatch.ElapsedMilliseconds,
                method.Children
            );

            if (_methodStack.Count > 0)
                _methodStack.Peek().Children.Add(result);
            else
                _rootMethods.Add(result);
        }

        public ThreadTraceResult GetResult()
        {
            return new ThreadTraceResult(
                _threadId,
                _rootMethods.Sum(m => m.Time),
                new List<MethodTraceResult>(_rootMethods)
            );
        }

        private class MethodInfo
        {
            public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
            public List<MethodTraceResult> Children { get; } = new();
        }
    }
}