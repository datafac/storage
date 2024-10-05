using DTOMaker.Models;
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
                    [EntityLayout(LayoutMethod.Explicit, 64)]
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
            generatorResult.GeneratedSources.Should().HaveCount(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            outputSource.HintName.Should().Be("MyOrg.Models.MyDTO.MemBlocks.g.cs");
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
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [MemberLayout(0)]
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
        public async Task Happy03_TwoMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [MemberLayout(0)] 
                        double Field1 { get; set; }

                        [Member(2)]
                        [MemberLayout(8)] 
                        long Field2 { get; set; }
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
        public async Task Happy04_TwoEntities()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyFirstDTO
                    {
                        [Member(1)]
                        [MemberLayout(0)] 
                        double Field1 { get; set; }
                    }

                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyOtherDTO
                    {
                        [Member(1)]
                        [MemberLayout(0)]
                        long Field1 { get; set; }
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
                outputSource.HintName.Should().Be("MyOrg.Models.MyFirstDTO.MemBlocks.g.cs");
            }
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];
                outputSource.HintName.Should().Be("MyOrg.Models.MyOtherDTO.MemBlocks.g.cs");
                string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
                await Verifier.Verify(outputCode);
            }
        }

        [Fact]
        public void Fault01_InvalidLayoutMethod()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Undefined, 64)]
                    public interface IMyDTO
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().StartWith("LayoutMethod is not defined.");
        }

        [Fact]
        public void Fault02_InvalidBlockSize()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 63)]
                    public interface IMyDTO
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().StartWith("BlockLength (63) is invalid.");
        }

        [Fact]
        public void Fault03_OrphanMember()
        {
            // note: both [Entity] and [EntityLayout] attributes missing
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [MemberLayout(0)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(0);
        }

        [Fact]
        public void Fault04_MissingEntityAttribute()
        {
            // note: [Entity] attribute is missing
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [MemberLayout(0)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().Be("[Entity] attribute is missing.");
        }

        [Fact]
        public void Fault05_MissingEntityLayoutAttribute()
        {
            // note: [EntityLayout] attribute is missing
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [MemberLayout(0)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(2);
            errors[0].GetMessage().Should().Be("[EntityLayout] attribute is missing.");
            errors[1].GetMessage().Should().Be("FieldLength (0) must be > 0");
        }

        [Fact]
        public void Fault06_MissingMemberAttribute()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [MemberLayout(0)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.Should().Be(2);
            errors[0].GetMessage().Should().Be("Expected member 'Field1' sequence to be 1, but found 0.");
            errors[1].GetMessage().Should().Be("[Member] attribute is missing.");
        }

        [Fact]
        public void Fault07_MissingMemberLayoutAttribute()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().Be("[MemberLayout] attribute is missing.");
        }

        [Fact]
        public void Fault08_InvalidMemberOffset_Lo()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [MemberLayout(-1)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.Should().Be(2);
            errors[0].GetMessage().Should().Be("This member extends before the start of the block.");
            errors[1].GetMessage().Should().Be("FieldOffset (-1) must be >= 0");
        }

        [Fact]
        public void Fault09_InvalidMemberOffset_Hi()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout(LayoutMethod.Explicit, 8)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [MemberLayout(4)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().Be("This member extends beyond the end of the block.");
        }

    }
}