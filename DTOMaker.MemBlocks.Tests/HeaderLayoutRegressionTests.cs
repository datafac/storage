using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.MemBlocks.Tests
{
    public class HeaderLayoutRegressionTests
    {
        private readonly string inputSource1 =
            """
            using System;
            using DTOMaker.Models;
            using DTOMaker.Models.MemBlocks;
            namespace MyOrg.Models
            {
                [Entity] [Layout(LayoutMethod.Linear, 64)]
                [Id(1)]
                public interface IHeader
                {
                    [Member(1)]  byte  Marker00  { get; set; } // '|'
                    [Member(2)]  byte  Marker01  { get; set; } // '_'
                    [Member(3)]  byte  MajorVer  { get; set; } // 1
                    [Member(4)]  byte  MinorVer  { get; set; } // 1
                    [Member(5)]  int   EntityId  { get; set; }
                    [Member(6)]  long  Structure { get; set; }
                }
            }
            """;

        [Fact]
        public void HeaderLayout00_GeneratedSourcesLengthShouldBe1()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.Header.MemBlocks.g.cs");
        }

        [Fact]
        public async Task HeaderLayout01_VerifyGeneratedSourceA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}