namespace Tracer.Core
{
    public class MethodTraceResult
    {
        public string MethodName { get; }
        public string ClassName { get; }
        public long Time { get; }
        public IReadOnlyList<MethodTraceResult> Methods { get; }

        internal MethodTraceResult(string methodName, string className, long time, List<MethodTraceResult> methods)
        {
            MethodName = methodName;
            ClassName = className;
            Time = time;
            Methods = methods;
        }
    }
}
