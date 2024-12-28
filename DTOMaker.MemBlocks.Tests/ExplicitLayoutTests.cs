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
    public class AttributeSyncTests
    {
        private void LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod a, DTOMaker.MemBlocks.LayoutMethod b)
        {
            int aOrdinal = (int)a;
            int bOrdinal = (int)b;
            bOrdinal.Should().Be(aOrdinal);
        }

        [Fact]
        public void LayoutMethodsMatch()
        {
            LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod.Undefined, DTOMaker.MemBlocks.LayoutMethod.Undefined);
            LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod.Explicit, DTOMaker.MemBlocks.LayoutMethod.Explicit);
            LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod.SequentialV1, DTOMaker.MemBlocks.LayoutMethod.SequentialV1);
        }

        [Fact]
        public void AttributeNamesMatch()
        {
            nameof(DTOMaker.MemBlocks.LayoutAttribute).Should().Be(nameof(DTOMaker.Models.MemBlocks.LayoutAttribute));
            nameof(DTOMaker.MemBlocks.IdAttribute).Should().Be(nameof(DTOMaker.Models.MemBlocks.IdAttribute));
            nameof(DTOMaker.MemBlocks.OffsetAttribute).Should().Be(nameof(DTOMaker.Models.MemBlocks.OffsetAttribute));
            nameof(DTOMaker.MemBlocks.LengthAttribute).Should().Be(nameof(DTOMaker.Models.MemBlocks.LengthAttribute));
            nameof(DTOMaker.MemBlocks.EndianAttribute).Should().Be(nameof(DTOMaker.Models.MemBlocks.EndianAttribute));
        }
    }
    public class ExplicitLayoutTests
    {
        [Fact]
        public async Task Happy01_NoMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
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
            generatorResult.GeneratedSources.Should().HaveCount(2);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];

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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(0)]
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
        public async Task Happy03_TwoMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(8)] 
                        double Field1 { get; set; }

                        [Member(2)]
                        [Offset(16)] 
                        long Field2 { get; set; }
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
        public async Task Happy04_TwoEntities()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyFirstDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyFirstDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
                        double Field1 { get; set; }
                    }

                    [Entity]
                    [Id("MyOtherDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyOtherDTO
                    {
                        [Member(1)]
                        [Offset(0)]
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
            generatorResult.GeneratedSources.Length.Should().Be(3);
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];
                outputSource.HintName.Should().Be("MyOrg.Models.MyFirstDTO.MemBlocks.g.cs");
            }
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[2];
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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Undefined, 64)]
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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 63)]
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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
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
            generatorResult.GeneratedSources.Length.Should().Be(1);
        }

        [Fact]
        public void Fault04_MissingEntityAttribute()
        {
            // note: [Entity] attribute is missing
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
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
            errors[1].GetMessage().Should().Be("FieldLength (0) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
        }

        [Fact]
        public void Fault06_MissingMemberAttribute()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Offset(0)]
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
        public void Fault07_MissingOffsetAttribute()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
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
            errors[0].GetMessage().Should().Be("[Offset] attribute is missing.");
            // 
        }

        [Fact]
        public void Fault08_InvalidMemberOffset_Lo()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(-1)]
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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 8)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(4)]
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

        [Fact]
        public void Fault10_InvalidMemberAlignment()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("MyDTO")][Layout(LayoutMethod.Explicit, 16)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(4)]
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
            errors[0].GetMessage().Should().Be("This member is incorrectly aligned. FieldOffset (4) modulo total length (8) must be 0.");
        }

    }
}