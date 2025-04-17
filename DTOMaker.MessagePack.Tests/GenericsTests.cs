using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.MessagePack.Tests
{
    public class GenericsTests
    {
        private readonly string source1 =
            """
            using System;
            using DTOMaker.Models;
            using DTOMaker.Models.MessagePack;
            namespace MyOrg.Models
            {
                [Entity][Id(1)]
                public interface IPair<T1, T2>
                {
                    [Member(1)] T1 Item1 {get;}
                    [Member(2)] T2 Item2 {get;}
                }
                [Entity][Id(2)]
                public interface IMyDTO2 : IPair<long, string>
                {
                    [Member(1)] int Id {get;}
                }
                [Entity][Id(3)]
                public interface IMyDTO3
                {
                    [Member(1)] int Id {get;}
                    [Member(2)] IPair<long, string>? Pair {get;}
                }
            }
            """;

        [Fact]
        public void Generic1_Pair1_CheckGeneratedSources()
        {
            var grr = GeneratorTestHelper.RunSourceGenerator(source1, LanguageVersion.LatestMajor);
            grr.Exception.ShouldBeNull();
            grr.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            grr.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            grr.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            grr.GeneratedSources.Length.ShouldBe(3);
            grr.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.MyDTO2.MessagePack.g.cs");
            grr.GeneratedSources[1].HintName.ShouldBe("MyOrg.Models.MyDTO3.MessagePack.g.cs");
            grr.GeneratedSources[2].HintName.ShouldBe("MyOrg.Models.Pair_2_Int64_String.MessagePack.g.cs");
        }

        [Fact]
        public async Task Generic1_Pair2_VerifySource0()
        {
            var grr = GeneratorTestHelper.RunSourceGenerator(source1, LanguageVersion.LatestMajor);
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);

        }

        [Fact]
        public async Task Generic1_Pair3_VerifySource1()
        {
            var grr = GeneratorTestHelper.RunSourceGenerator(source1, LanguageVersion.LatestMajor);
            string outputCode = grr.GeneratedSources[1].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic1_Pair4_VerifySource2()
        {
            var grr = GeneratorTestHelper.RunSourceGenerator(source1, LanguageVersion.LatestMajor);
            string outputCode = grr.GeneratedSources[2].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        private readonly string source2 =
            """
            using System;
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Models.MessagePack;
            namespace MyOrg.Models
            {
                [Entity][Id(1)]
                public interface IMonoid<T1>
                {
                    [Member(1)] T1 Value {get;}
                }
                [Entity][Id(2)]
                public interface IMyDTO : IMonoid<_T1_>
                {
                }
            }
            """;

        [Fact]
        public void Generic2_Monoid1_CheckGeneratedSources()
        {
            string source = source2.Replace("_T1_", "bool");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Exception.ShouldBeNull();
            grr.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            grr.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            grr.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            grr.GeneratedSources.Length.ShouldBe(2);
            grr.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.Monoid_1_Boolean.MessagePack.g.cs");
            grr.GeneratedSources[1].HintName.ShouldBe("MyOrg.Models.MyDTO.MessagePack.g.cs");
        }

        [Fact]
        public async Task Generic2_Monoid2_bool()
        {
            string source = source2.Replace("_T1_", "bool");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_byte()
        {
            string source = source2.Replace("_T1_", "byte");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_sbyte()
        {
            string source = source2.Replace("_T1_", "sbyte");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_short()
        {
            string source = source2.Replace("_T1_", "short");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_ushort()
        {
            string source = source2.Replace("_T1_", "ushort");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_char()
        {
            string source = source2.Replace("_T1_", "char");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_Half()
        {
            string source = source2.Replace("_T1_", "Half");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_int()
        {
            string source = source2.Replace("_T1_", "int");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_uint()
        {
            string source = source2.Replace("_T1_", "uint");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_float()
        {
            string source = source2.Replace("_T1_", "float");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_long()
        {
            string source = source2.Replace("_T1_", "long");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_ulong()
        {
            string source = source2.Replace("_T1_", "ulong");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_double()
        {
            string source = source2.Replace("_T1_", "double");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_string()
        {
            string source = source2.Replace("_T1_", "string");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_Guid()
        {
            string source = source2.Replace("_T1_", "Guid");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_decimal()
        {
            string source = source2.Replace("_T1_", "decimal");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Generic2_Monoid2_Octets()
        {
            string source = source2.Replace("_T1_", "Octets");
            var grr = GeneratorTestHelper.RunSourceGenerator(source, LanguageVersion.LatestMajor);
            grr.Diagnostics.ShouldBeEmpty();
            string outputCode = grr.GeneratedSources[0].SourceText.ToString();
            await Verifier.Verify(outputCode);
        }

    }
}