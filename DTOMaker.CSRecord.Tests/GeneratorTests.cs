using Shouldly;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.CSRecord.Tests
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
                    [Entity][Id(1)]
                    public interface IMyDTO
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.ShouldBe("MyOrg.Models.MyDTO.CSRecord.g.cs");
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
                    [Entity][Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.ShouldBe("MyOrg.Models.MyDTO.CSRecord.g.cs");
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
                    [Entity][Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                        [Member(2)] long Field2 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.ShouldBe("MyOrg.Models.MyDTO.CSRecord.g.cs");
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
                    [Entity][Id(1)]
                    public interface IMyFirstDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                    }

                    [Entity][Id(2)]
                    public interface IMyOtherDTO
                    {
                        [Member(1)] long Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(2);
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];
                outputSource.HintName.ShouldBe("MyOrg.Models.MyFirstDTO.CSRecord.g.cs");
            }
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];
                outputSource.HintName.ShouldBe("MyOrg.Models.MyOtherDTO.CSRecord.g.cs");
                string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
                await Verifier.Verify(outputCode);
            }
        }

        [Fact]
        public async Task Happy05_ArrayMember()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity][Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] ReadOnlyMemory<double> Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.ShouldBe("MyOrg.Models.MyDTO.CSRecord.g.cs");
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Happy06_StringMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity][Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] string  FamilyName { get; set; }
                        [Member(2)] string  GivenName  { get; set; }
                        [Member(3)] string? OtherNames { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.ShouldBe("MyOrg.Models.MyDTO.CSRecord.g.cs");
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
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(0);
        }

    }
}