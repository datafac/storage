using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DTOMaker.Gentime;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Json;

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

    public sealed class SimpleDynaText : DynaTextBase, ISimple
    {
        public string Field1
        {
            get => _map.Get<string>(nameof(Field1), "");
            set => _map.Set<string>(nameof(Field1), value);
        }
        public int Field2
        {
            get => _map.Get<int>(nameof(Field2), 0);
            set => _map.Set<int>(nameof(Field2), value);
        }
    }

    public interface INested
    {
        string Field1 { get; set; }
        int Field2 { get; set; }
    }

    public sealed class NestedJsonPoco : INested
    {
        public string Field1 { get; set; } = "";
        public int Field2 { get; set; }
    }

    public sealed class NestedDynaText : DynaTextBase, INested
    {
        public string Field1
        {
            get => _map.Get<string>(nameof(Field1), "");
            set => _map.Set<string>(nameof(Field1), value);
        }
        public int Field2
        {
            get => _map.Get<int>(nameof(Field2), 0);
            set => _map.Set<int>(nameof(Field2), value);
        }
    }

    [MemoryDiagnoser]
    //[SimpleJob(RuntimeMoniker.Net481)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class DynaTextVsNewtonSoftJson
    {
        [GlobalSetup]
        public void GlobalSetup()
        {
        }

        [Benchmark(Baseline = true)]
        public int RoundtripJsonPoco()
        {
            var orig = new SimpleJsonPoco();
            orig.Field1 = "abcdef";
            orig.Field2 = 123;
            string buffer = JsonConvert.SerializeObject(orig);
            SimpleJsonPoco copy = JsonConvert.DeserializeObject<SimpleJsonPoco>(buffer) ?? throw new Exception();
            if (copy.Field1 != orig.Field1) throw new Exception();
            if (copy.Field2 != orig.Field2) throw new Exception();
            return buffer.Length;
        }

        [Benchmark]
        public int RoundtripDynaText()
        {
            var orig = new SimpleDynaText();
            orig.Field1 = "abcdef";
            orig.Field2 = 123;
            string buffer = orig.EmitText();
            var copy = new SimpleDynaText();
            copy.LoadFrom(buffer);
            if (copy.Field1 != orig.Field1) throw new Exception();
            if (copy.Field2 != orig.Field2) throw new Exception();
            return buffer.Length;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<DynaTextVsNewtonSoftJson>();
        }
    }
}
