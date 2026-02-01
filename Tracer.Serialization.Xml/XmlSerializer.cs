using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Xml
{
    public class XmlResultSerializer : ITraceResultSerializer
    {
        public string Format => "xml";

        private readonly XmlSerializer _serializer;
        private readonly XmlWriterSettings _settings;
        private readonly XmlSerializerNamespaces _emptyNamespaces;

        public XmlResultSerializer()
        {
            _serializer = new XmlSerializer(typeof(TraceResultDto));

            _settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = false
            };

            _emptyNamespaces = new XmlSerializerNamespaces();
            _emptyNamespaces.Add("", "");
        }

        public void Serialize(TraceResult traceResult, Stream to)
        {
            if (traceResult == null) throw new ArgumentNullException(nameof(traceResult));
            if (to == null) throw new ArgumentNullException(nameof(to));

            var dto = TraceResultDto.MapToSerializableDto(traceResult);

            using (var writer = XmlWriter.Create(to, _settings))
            {
                _serializer.Serialize(writer, dto, _emptyNamespaces);
            }
        }
    }
    
    [XmlRoot("root")]
    public class TraceResultDto
    {
        [XmlElement("thread")]
        public List<ThreadDto> Threads { get; set; } = new List<ThreadDto>();

        public TraceResultDto() { }

        public static TraceResultDto MapToSerializableDto(TraceResult source)
        {
            return new TraceResultDto
            {
                Threads = source.Threads.Select(ThreadDto.MapToSerializableDto).ToList()
            };
        }
    }

    public class ThreadDto
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("time")]
        public string Time { get; set; }

        [XmlElement("method")]
        public List<MethodDto> Methods { get; set; } = new List<MethodDto>();

        public ThreadDto() { }

        public static ThreadDto MapToSerializableDto(ThreadTraceResult source)
        {
            return new ThreadDto
            {
                Id = source.ThreadId,
                Time = $"{source.Time}ms",
                Methods = source.Methods.Select(MethodDto.MapToSerializableDto).ToList()
            };
        }
    }

    public class MethodDto
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("class")]
        public string ClassName { get; set; }

        [XmlAttribute("time")]
        public string Time { get; set; }

        [XmlElement("method")]
        public List<MethodDto> Methods { get; set; } = new List<MethodDto>();

        public MethodDto() { }

        public static MethodDto MapToSerializableDto(MethodTraceResult source)
        {
            return new MethodDto
            {
                Name = source.MethodName,
                ClassName = source.ClassName,
                Time = $"{source.Time}ms",
                Methods = source.Methods.Select(MapToSerializableDto).ToList()
            };
        }
    }
}