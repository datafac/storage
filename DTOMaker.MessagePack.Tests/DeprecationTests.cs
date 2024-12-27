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
    public class DeprecationTests
    {
        [Fact]
        public async Task ObsoleteMember01()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.Models
                {
                    [Entity][EntityKey(1)]
                    public interface IMyDTO
                    {
                        [Obsolete]
                        [Member(1)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Should().HaveCount(2);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task ObsoleteMember02()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.Models
                {
                    [Entity][EntityKey(1)]
                    public interface IMyDTO
                    {
                        [Obsolete("Removed")]
                        [Member(1)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Should().HaveCount(2);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task ObsoleteMember03()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.Models
                {
                    [Entity][EntityKey(1)]
                    public interface IMyDTO
                    {
                        [Obsolete("Removed", true)]
                        [Member(1)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Should().HaveCount(2);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}