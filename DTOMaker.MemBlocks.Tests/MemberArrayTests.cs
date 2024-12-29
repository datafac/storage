using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.MemBlocks.Tests
{
    public class MemberArrayTests
    {
        [Fact]
        public async Task Array01_FixedLength()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Length(8)]
                        ReadOnlyMemory<double> Values { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Should().HaveCount(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public void Array02_InvalidLength()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Length(3)]
                        ReadOnlyMemory<double> Values { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(2);
            errors[0].GetMessage().Should().Be("ArrayLength (3) is invalid. ArrayLength must be a whole power of 2 between 1 and 1024.");
            errors[1].GetMessage().Should().Be("Total length (24) is invalid. Total length must be a whole power of 2 between 1 and 1024.");
        }

        [Fact]
        public void Array03_TooLarge()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Length(256)]
                        ReadOnlyMemory<double> Values { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().Be("Total length (2048) is invalid. Total length must be a whole power of 2 between 1 and 1024.");
        }

        [Fact]
        public async Task Array04_BufferOfBytes()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Length(8)]
                        ReadOnlyMemory<byte> Values { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Should().HaveCount(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}