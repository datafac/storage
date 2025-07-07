using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
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
            bOrdinal.ShouldBe(aOrdinal);
        }

        [Fact]
        public void LayoutMethodsMatch()
        {
            LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod.Undefined, DTOMaker.MemBlocks.LayoutMethod.Undefined);
            LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod.Explicit, DTOMaker.MemBlocks.LayoutMethod.Explicit);
            LayoutMethodsAreEquivalent(DTOMaker.Models.MemBlocks.LayoutMethod.Linear, DTOMaker.MemBlocks.LayoutMethod.Linear);
        }

        [Fact]
        public void AttributeNamesMatch()
        {
            nameof(DTOMaker.MemBlocks.LayoutAttribute).ShouldBe(nameof(DTOMaker.Models.MemBlocks.LayoutAttribute));
            nameof(DTOMaker.MemBlocks.OffsetAttribute).ShouldBe(nameof(DTOMaker.Models.MemBlocks.OffsetAttribute));
            nameof(DTOMaker.MemBlocks.EndianAttribute).ShouldBe(nameof(DTOMaker.Models.MemBlocks.EndianAttribute));
            nameof(DTOMaker.MemBlocks.FixedLengthAttribute).ShouldBe(nameof(DTOMaker.Models.MemBlocks.FixedLengthAttribute));
            nameof(DTOMaker.MemBlocks.CapacityAttribute).ShouldBe(nameof(DTOMaker.Models.MemBlocks.CapacityAttribute));
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
                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
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
            outputSource.HintName.ShouldBe("MyOrg.Models.MyDTO.MemBlocks.g.cs");
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
                    [Entity] [Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(0)]
                        double Field1 { get; set; }
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
                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
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
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
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
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
                    public interface IMyFirstDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
                        double Field1 { get; set; }
                    }

                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(2)]
                    public interface IMyOtherDTO
                    {
                        [Member(1)]
                        [Offset(0)]
                        long Field1 { get; set; }
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
                outputSource.HintName.ShouldBe("MyOrg.Models.MyFirstDTO.MemBlocks.g.cs");
            }
            {
                GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];
                outputSource.HintName.ShouldBe("MyOrg.Models.MyOtherDTO.MemBlocks.g.cs");
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
                    [Entity] [Layout((LayoutMethod)3, 64)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("LayoutMethod (3) is not supported.");
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
                    [Entity] [Layout(LayoutMethod.Explicit, 63)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldStartWith("BlockLength (63) is invalid.");
        }

        [Fact]
        public void Fault03_OrphanMember()
        {
            // note: both [Entity] and [Layout] attributes missing
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
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(0);
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
                    [Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("Entity identifier must be unique positive number. Have you forgotten the entity [Id] attribute?");
        }

        [Fact]
        public void Fault05_MissingLayoutAttribute()
        {
            // note: [Layout] attribute is missing
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(2);
            errors[0].GetMessage().ShouldBe("LayoutMethod is not defined. Is the [Layout] attribute missing?");
            errors[1].GetMessage().ShouldBe("FieldLength (0) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
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
                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Offset(0)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(2);
            errors[0].GetMessage().ShouldBe("Expected member 'Field1' sequence to be 1, but found 0.");
            errors[1].GetMessage().ShouldBe("[Member] attribute is missing.");
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
                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("[Offset] attribute is missing. This is required for Explicit layout method.");
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
                    [Entity][Layout(LayoutMethod.Explicit, 64)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(-1)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(2);
            errors[0].GetMessage().ShouldBe("This member extends before the start of the block.");
            errors[1].GetMessage().ShouldBe("FieldOffset (-1) must be >= 0");
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
                    [Entity][Layout(LayoutMethod.Explicit, 8)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(4)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("This member extends beyond the end of the block.");
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
                    [Entity][Layout(LayoutMethod.Explicit, 16)]
                    [Id(1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Offset(4)]
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("This member is incorrectly aligned. FieldOffset (4) modulo total length (8) must be 0.");
        }

        [Fact]
        public void Fault11_MissingIdAttribute()
        {
            // note: [Id] attribute is missing
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Layout(LayoutMethod.Explicit, 64)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Offset(0)] 
                        double Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("Entity identifier must be unique positive number. Have you forgotten the entity [Id] attribute?");
        }

    }
}