namespace Tracer.Core
{
    public class ThreadTraceResult
    {
        public int ThreadId { get; }
        public long Time { get; }
        public IReadOnlyList<MethodTraceResult> Methods { get; }

        internal ThreadTraceResult(int threadId, long time, List<MethodTraceResult> methods)
        {
            ThreadId = threadId;
            Time = time;
            Methods = methods;
        }
    }
}