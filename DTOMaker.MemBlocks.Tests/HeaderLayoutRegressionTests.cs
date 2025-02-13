using Shouldly;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;
using System;
using VerifyXunit;

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
                [Id("01234567-89ab-cdef-0123-456789abcdef")]
                public interface IHeader
                {
                    [Member(1)]  byte  MarkerByte0 { get; set; } // '|'
                    [Member(2)]  byte  MarkerByte1 { get; set; } // '_'
                    [Member(3)]  byte  HeaderMajorVersion { get; set; }
                    [Member(4)]  byte  HeaderMinorVersion { get; set; }
                    [Member(5)]  byte  SpareByte0  { get; set; }
                    [Member(6)]  byte  SpareByte1  { get; set; }
                    [Member(7)]  byte  SpareByte2  { get; set; }
                    [Member(8)]  byte  SpareByte3  { get; set; }
                    [Member(9)]  byte  ClassHeight { get; set; }
                    [Member(10)] byte  BlockSize1 { get; set; }
                    [Member(11)] byte  BlockSize2 { get; set; }
                    [Member(12)] byte  BlockSize3 { get; set; }
                    [Member(13)] byte  BlockSize4 { get; set; }
                    [Member(14)] byte  BlockSize5 { get; set; }
                    [Member(15)] byte  BlockSize6 { get; set; }
                    [Member(16)] byte  BlockSize7 { get; set; }
                    [Member(17)] Guid  EntityGuid { get; set; }
                    [Member(18)] Guid  SpareGuid0 { get; set; }
                    [Member(19)] Guid  SpareGuid1 { get; set; }
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