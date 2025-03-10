using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.CSPoco.Tests
{
    public class BinaryMemberTests
    {
        private readonly string inputSource1 =
            """
            using DataFac.Memory;
            using DTOMaker.Models;
            namespace MyOrg.Models
            {
                [Entity][Id(2)] public interface IOther
                {
                    [Member(1)] Int64  Value1 { get; set; }
                    [Member(2)] Int64  Value2 { get; set; }
                }
                [Entity][Id(1)] public interface IMyDTO
                {
                    [Member(1)] IOther? Other1 { get; set; }
                    [Member(2)] Octets  Field1 { get; set; }
                    [Member(3)] Octets? Field2 { get; set; }
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
            generatorResult.GeneratedSources.Length.ShouldBe(2);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.MyDTO.CSPoco.g.cs");
            generatorResult.GeneratedSources[1].HintName.ShouldBe("MyOrg.Models.Other.CSPoco.g.cs");
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

        [Fact]
        public async Task BinaryMember02_VerifyGeneratedSourceB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[1];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}