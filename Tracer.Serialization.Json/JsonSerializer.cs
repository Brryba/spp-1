using System.Text.Json;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Json
{
    public class JsonTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "json";

        public void Serialize(TraceResult traceResult, Stream to)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true 
            };

            JsonSerializer.Serialize(to, traceResult, options);
        }
    }
}