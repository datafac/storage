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
    }
}