using Shouldly;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.MemBlocks.Tests
{
    public class LinearLayoutTests
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
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
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
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
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
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[0];

            // custom generation checks
            string outputCode = string.Join(Environment.NewLine, outputSource.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Happy03_ThreeMembers()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                        [Member(2)] bool Field2 { get; set; }
                        [Member(3)] long Field3 { get; set; }
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
        public async Task Happy06_ObsoleteMember()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Obsolete("Removed", true)]
                        [Member(1)] 
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
        public async Task Happy07_StringMembers()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [StrLen(64)]
                        string FamilyName { get; set; }

                        [Member(2)]
                        [StrLen(64)]
                        string GivenNames { get; set; }

                        [Member(3)]
                        [StrLen(64)]
                        string OtherNames_Value { get; set; }

                        [Member(4)] bool OtherNames_HasValue { get; set; }

                        string? OtherNames { get; set; }
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
        public async Task Happy98_AllTypes()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)]  bool    Field1  { get; set; }
                        [Member(2)]  sbyte   Field2  { get; set; }
                        [Member(3)]  byte    Field3  { get; set; }
                        [Member(4)]  short   Field4  { get; set; }
                        [Member(5)]  ushort  Field5  { get; set; }
                        [Member(6)]  char    Field6  { get; set; }
                        [Member(7)]  Half    Field7  { get; set; }
                        [Member(8)]  int     Field8  { get; set; }
                        [Member(9)]  uint    Field9  { get; set; }
                        [Member(10)] float   Field10 { get; set; }
                        [Member(11)] long    Field11 { get; set; }
                        [Member(12)] ulong   Field12 { get; set; }
                        [Member(13)] double  Field13 { get; set; }
                        [Member(14)] Guid    Field14 { get; set; }
                        [Member(15)] Int128  Field15 { get; set; }
                        [Member(16)] UInt128 Field16 { get; set; }
                        [Member(17)] Decimal Field17 { get; set; }
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
        public void Fault01_Unsupported_Enums()
        {
            var inputSource =
                """
                using System;
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")]
                    [Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        DayOfWeek Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(2);
            errors[0].GetMessage().ShouldBe("MemberType 'System.DayOfWeek' not supported");
            errors[1].GetMessage().ShouldStartWith("FieldLength (0) is invalid");
        }

        [Fact]
        public void Fault02_Unsupported_NullValType()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        int? Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("Nullable type 'System.Int32?' is not supported.");
        }

        [Fact]
        public void Fault03_Unsupported_NullRefType()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        string? Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(3);
            errors[0].GetMessage().ShouldBe("Nullable type 'System.String?' is not supported.");
            errors[1].GetMessage().ShouldBe("FieldLength (0) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
            errors[2].GetMessage().ShouldBe("StringLength (0) is invalid. StringLength must be a whole power of 2 between 1 and 1024.");
        }

        [Fact]
        public void Fault04_Invalid_StringLength()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")][Layout(LayoutMethod.Linear)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [StrLen(31)]
                        string Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(2);
            errors[0].GetMessage().ShouldBe("FieldLength (31) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
            errors[1].GetMessage().ShouldBe("StringLength (31) is invalid. StringLength must be a whole power of 2 between 1 and 1024.");
        }

        [Fact]
        public void Fault05_EntityId_NotUnique()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity][Layout(LayoutMethod.Linear)]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")]
                    public interface IMyDTO1 { }

                    [Entity][Layout(LayoutMethod.Linear)]
                    [Id("01234567-89ab-cdef-0123-456789abcdef")]
                    public interface IMyDTO2 { }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("Entity id (01234567-89ab-cdef-0123-456789abcdef) is already used by entity: MyOrg.Models.MyDTO1");
        }

    }
}