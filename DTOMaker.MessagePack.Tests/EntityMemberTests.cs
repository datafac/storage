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
    public class EntityMemberTests
    {
        private readonly string inputSource1 =
            """
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.DomainA
                {
                    [Entity] [EntityKey(1)] public interface IMyDTO1
                    {
                        [Member(1)] long Field1 { get; set; }
                    }
                }
                namespace MyOrg.DomainB
                {
                    [Entity] [EntityKey(2)] public interface IMyDTO2
                    {
                        [Member(1)] MyOrg.DomainA.IMyDTO1? Member1 { get; set; }
                    }
                }
                """;

        [Fact]

        public async Task EntityMember01_OptionalEntityMemberA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(2);
            generatorResult.GeneratedSources[0].HintName.Should().Be("MyOrg.DomainA.MyDTO1.MessagePack.g.cs");
            generatorResult.GeneratedSources[1].HintName.Should().Be("MyOrg.DomainB.MyDTO2.MessagePack.g.cs");

            var source = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]

        public async Task EntityMember02_OptionalEntityMemberB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(2);
            generatorResult.GeneratedSources[0].HintName.Should().Be("MyOrg.DomainA.MyDTO1.MessagePack.g.cs");
            generatorResult.GeneratedSources[1].HintName.Should().Be("MyOrg.DomainB.MyDTO2.MessagePack.g.cs");

            var source = generatorResult.GeneratedSources[1];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}