using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using System;

namespace DynaText.Benchmarks
{
    public interface ISimple
    {
        string Field1 { get; set; }
        int Field2 { get; set; }
    }

    public sealed class SimpleJsonPoco : ISimple
    {
        public string Field1 { get; set; } = "";
        public int Field2 { get; set; }
    }

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net481)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class DynaTextVsNewtonSoftJson
    {
        [Benchmark(Baseline = true)]
        public int RoundtripJsonPoco()
        {
            var orig = new SimpleJsonPoco();
            orig.Field1 = "abcdef";
            orig.Field2 = 123;
            string buffer = JsonConvert.SerializeObject(orig);
            var copy = JsonConvert.DeserializeObject<SimpleJsonPoco>(buffer);
            return buffer.Length;
        }

        // todo bench competitor here

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<DynaTextVsNewtonSoftJson>();
        }
    }
}
