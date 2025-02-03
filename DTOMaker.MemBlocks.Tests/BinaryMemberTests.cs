using Shouldly;
using Microsoft.CodeAnalysis;
using System.Linq;
using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Microsoft.CodeAnalysis.CSharp;

namespace DTOMaker.MemBlocks.Tests
{
    public class BinaryMemberTests
    {
        private readonly string inputSource1 =
            """
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Models.MemBlocks;
            namespace MyOrg.Models
            {
                [Entity] [Layout(LayoutMethod.Linear)] public interface IMyDTO
                {
                    [Member(1)] Octets  Field1 { get; set; }
                    [Member(2)] Octets? Field2 { get; set; }
                }
            }
            """;

        [Fact]
        public void BinaryMember00_GeneratedSourcesLengthShouldBe1()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.MyDTO.MemBlocks.g.cs");
        }

        [Fact]
        public async Task BinaryMember01_VerifyGeneratedSourceA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}