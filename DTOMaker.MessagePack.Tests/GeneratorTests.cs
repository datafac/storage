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
    public class GeneratorTests
    {
        [Fact]
        public async Task Happy01_NoMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    public interface IMyDTO
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.Should().Be("MyOrg.Models.MyDTO.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Happy02_OneMember()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity()]
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.Should().Be("MyOrg.Models.MyDTO.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Happy03_TwoMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity()]
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                        [Member(2)] long Field2 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.Should().Be("MyOrg.Models.MyDTO.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Happy04_TwoEntities()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity()]
                    public interface IMyFirstDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                    }

                    [Entity()]
                    public interface IMyOtherDTO
                    {
                        [Member(1)] long Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(2);
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];
                outputSource.HintName.Should().Be("MyOrg.Models.MyFirstDTO.MessagePack.g.cs");
            }
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];
                outputSource.HintName.Should().Be("MyOrg.Models.MyOtherDTO.MessagePack.g.cs");
                string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
                await Verifier.Verify(outputCode);
            }
        }

        [Fact]
        public async Task Happy05_Enum32Member()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    public enum Kind32 : int
                    {
                        Default,
                        Kind1 = 1,
                        MaxKind = int.MaxValue,
                    }
                    [Entity]
                    public interface IMyDTO
                    {
                        [Member(1)] Kind32 Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.Should().Be("MyOrg.Models.MyDTO.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Happy06_ObsoleteMember()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
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
            generatorResult.GeneratedSources.Should().HaveCount(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public void Fault01_OrphanMember()
        {
            // note: [Entity] attribute is missing
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Should().BeEmpty();
        }

    }
}