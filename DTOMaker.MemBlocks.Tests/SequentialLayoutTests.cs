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
    public class SequentialLayoutTests
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
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
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            generatorResult.GeneratedSources.Should().HaveCount(2);
            GeneratedSourceResult outputSource = generatorResult.GeneratedSources[1];

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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] double Field1 { get; set; }
                        [Member(2)] bool Field2 { get; set; }
                        [Member(3)] long Field3 { get; set; }
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)]
                        [Length(64)]
                        string FamilyName { get; set; }

                        [Member(2)]
                        [Length(64)]
                        string GivenNames { get; set; }

                        [Member(3)]
                        [Length(64)]
                        string OtherNames_Value { get; set; }

                        [Member(4)] bool OtherNames_HasValue { get; set; }

                        string? OtherNames { get; set; }
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
        public async Task Happy98_AllTypes()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
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
        public void Fault01_Unsupported_Enums()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity]
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        DayOfWeek Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(2);
            errors[0].GetMessage().Should().Be("MemberType 'DayOfWeek' not supported");
            errors[1].GetMessage().Should().StartWith("FieldLength (0) is invalid");
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        int? Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().Be("Nullable type 'Int32?' is not supported.");
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        string? Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(2);
            errors[0].GetMessage().Should().Be("Nullable type 'String?' is not supported.");
            errors[1].GetMessage().Should().Be("FieldLength (0) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
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
                    [EntityLayout("MyDTO", LayoutMethod.SequentialV1)]
                    public interface IMyDTO
                    {
                        [Member(1)] 
                        [Length(31)]
                        string Field1 { get; set; }
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Should().HaveCount(1);
            errors[0].GetMessage().Should().Be("FieldLength (31) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
        }

    }
}