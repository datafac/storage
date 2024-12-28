using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.MessagePack.Tests
{
    public class MultiDomainTests
    {
        private readonly string inputSource =
            """
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.DomainA
                {
                    [Entity] [EntityKey(1)] public interface IMyDTO { }
                }
                namespace MyOrg.DomainB
                {
                    [Entity] [EntityKey(2)] public interface IMyDTO { }
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
            generatorResult.GeneratedSources.Length.Should().Be(3);
            generatorResult.GeneratedSources[0].HintName.Should().Be("DTOMaker.Common.EntityBase.MessagePack.g.cs");
            generatorResult.GeneratedSources[1].HintName.Should().Be("MyOrg.DomainA.MyDTO.MessagePack.g.cs");
            generatorResult.GeneratedSources[2].HintName.Should().Be("MyOrg.DomainB.MyDTO.MessagePack.g.cs");
        }

        [Fact]
        public async Task Domains02_2Entities_VerifyBase()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.Should().Be(3);
            var source = generatorResult.GeneratedSources[0];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Domains03_2Entities_VerifyDomainA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.Should().Be(3);
            var source = generatorResult.GeneratedSources[1];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Domains04_2Entities_VerifyDomainB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.Should().Be(3);
            var source = generatorResult.GeneratedSources[2];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}