using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.CSPoco.Tests
{
    public class MultiDomainTests
    {
        private readonly string inputSource =
            """
                using DTOMaker.Models;
                namespace MyOrg.DomainA
                {
                    [Entity] public interface IMyDTO { }
                }
                namespace MyOrg.DomainB
                {
                    [Entity] public interface IMyDTO { }
                }
                """;

        [Fact]
        public void Domains01_2Entities_Generates3Outputs()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(2);
            generatorResult.GeneratedSources[0].HintName.Should().Be("MyOrg.DomainA.MyDTO.CSPoco.g.cs");
            generatorResult.GeneratedSources[1].HintName.Should().Be("MyOrg.DomainB.MyDTO.CSPoco.g.cs");
        }

        [Fact]
        public async Task Domains03_2Entities_VerifyDomainA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.Should().Be(2);
            var source = generatorResult.GeneratedSources[0];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Domains04_2Entities_VerifyDomainB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.Should().Be(2);
            var source = generatorResult.GeneratedSources[1];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}