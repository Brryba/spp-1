namespace Tracer.Core
{
    public class TraceResult
    {
        public IReadOnlyList<ThreadTraceResult> Threads { get; }

        public TraceResult(List<ThreadTraceResult> threads)
        {
            Threads = threads;
        }
    }
}