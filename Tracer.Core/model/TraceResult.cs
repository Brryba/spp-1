namespace Tracer.Core
{
    public class TraceResult
    {
        public IReadOnlyList<ThreadTraceResult> Threads { get; }

        internal TraceResult(List<ThreadTraceResult> threads)
        {
            Threads = threads;
        }
    }
}