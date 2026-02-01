using Tracer.Core;

namespace Tracer.Tests
{
    public class TracerTests
    {
        [Fact]
        public void SingleThread_ShouldTraceMethodName_AndPositiveTime()
        {
            ITracer tracer = new Core.Tracer();
            var testObj = new TestClass(tracer);

            testObj.SimpleMethod();
            var result = tracer.GetTraceResult();

            Assert.NotNull(result);
            Assert.Single(result.Threads); 

            var threadResult = result.Threads[0];
            Assert.Single(threadResult.Methods);

            var methodResult = threadResult.Methods[0];
            Assert.Equal(nameof(TestClass.SimpleMethod), methodResult.MethodName); 
            Assert.Equal(nameof(TestClass), methodResult.ClassName); 
            Assert.True(methodResult.Time >= 0);
        }

        [Fact]
        public void NestedCalls_ShouldHaveCorrectStructure()
        {
            ITracer tracer = new Core.Tracer();
            var testObj = new TestClass(tracer);

            testObj.MethodA(); 
            var result = tracer.GetTraceResult();

            var rootMethod = result.Threads[0].Methods[0];
            
            Assert.Equal(nameof(TestClass.MethodA), rootMethod.MethodName);
            Assert.Single(rootMethod.Methods);

            var childMethod = rootMethod.Methods[0];
            Assert.Equal(nameof(TestClass.MethodB), childMethod.MethodName); 
        }

        [Fact]
        public void MultiThread_ShouldSeparateTraceResults()
        {
            ITracer tracer = new Core.Tracer();
            var testObj = new TestClass(tracer);
            
            var t1 = new Thread(() => testObj.SimpleMethod());
            var t2 = new Thread(() => testObj.MethodA());

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            var result = tracer.GetTraceResult();

            Assert.Equal(2, result.Threads.Count);
            
            var id1 = result.Threads[0].ThreadId;
            var id2 = result.Threads[1].ThreadId;
            Assert.NotEqual(id1, id2);
        }
    }

    public class TestClass
    {
        private readonly ITracer _tracer;

        public TestClass(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void SimpleMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(10);
            _tracer.StopTrace();
        }

        public void MethodA()
        {
            _tracer.StartTrace();
            MethodB();
            _tracer.StopTrace();
        }

        public void MethodB()
        {
            _tracer.StartTrace();
            Thread.Sleep(10);
            _tracer.StopTrace();
        }
    }
}