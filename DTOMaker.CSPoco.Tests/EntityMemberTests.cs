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
    public class EntityMemberTests
    {
        private readonly string inputSource1 =
            """
                using DTOMaker.Models;
                namespace MyOrg.DomainA
                {
                    [Entity] public interface IMyDTO1
                    {
                        [Member(1)] long Field1 { get; set; }
                    }
                }
                namespace MyOrg.DomainB
                {
                    [Entity] public interface IMyDTO1
                    {
                        [Member(1)] double Field1 { get; set; }
                    }
                }
                namespace MyOrg.DomainC
                {
                    [Entity] public interface IMyDTO2
                    {
                        [Member(1)] MyOrg.DomainA.IMyDTO1? Member1 { get; set; }
                        [Member(2)] MyOrg.DomainB.IMyDTO1  Member2 { get; set; }
                    }
                }
                """;

        [Fact]
        public void EntityMember00_GeneratedSourcesLengthShouldBe3()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(3);
            generatorResult.GeneratedSources[0].HintName.Should().Be("MyOrg.DomainA.MyDTO1.CSPoco.g.cs");
            generatorResult.GeneratedSources[1].HintName.Should().Be("MyOrg.DomainB.MyDTO1.CSPoco.g.cs");
            generatorResult.GeneratedSources[2].HintName.Should().Be("MyOrg.DomainC.MyDTO2.CSPoco.g.cs");
        }

        [Fact]
        public async Task EntityMember01_VerifyGeneratedSourceA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task EntityMember02_VerifyGeneratedSourceB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[1];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task EntityMember03_VerifyGeneratedSourceC()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[2];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}