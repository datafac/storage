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
    public class FixLenStringMemberTests
    {
        private readonly string inputSource1 =
            """
            using System;
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Models.MemBlocks;
            namespace MyOrg.Models
            {
                [Entity][Id(1)][Layout(LayoutMethod.Linear)]
                public interface IMyDTO
                {
                    [Member(1)] [FixedLength(128)] string Field1 { get; set; }
                    [Member(2)] [FixedLength(32)] string? Field2 { get; set; }
                }
            }
            """;

        [Fact]
        public void FixLenStringMember00_GeneratedSourcesLengthShouldBe1()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(1);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.MyDTO.MemBlocks.g.cs");
        }

        [Fact]
        public async Task FixLenStringMember01_VerifyGeneratedSourceA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource1, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        private readonly string inputSource2 =
            """
            using System;
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Models.MemBlocks;
            namespace MyOrg.Models
            {
                [Entity][Id(1)][Layout(LayoutMethod.Linear)]
                public interface IMyDTO
                {
                    [Member(1)] [FixedLength(2)] string Field1 { get; set; }
                }
            }
            """;

        [Fact]
        public void FixLenStringMember02_FailsWhenFixedLengthTooSmall()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource2, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(1);
            errors[0].GetMessage().ShouldBe("FixedLength (2) is invalid. FixedLength must be a whole power of 2 between 4 and 1024.");

        }

        private readonly string inputSource3 =
            """
            using System;
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Models.MemBlocks;
            namespace MyOrg.Models
            {
                [Entity][Id(1)][Layout(LayoutMethod.Linear)]
                public interface IMyDTO
                {
                    [Member(1)] [FixedLength(2048)] string Field1 { get; set; }
                }
            }
            """;

        [Fact]
        public void FixLenStringMember03_FailsWhenFixedLengthTooLarge()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource3, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();

            var errors = generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            errors.Length.ShouldBe(2);
            errors[0].GetMessage().ShouldBe("FieldLength (2048) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
            errors[1].GetMessage().ShouldBe("FixedLength (2048) is invalid. FixedLength must be a whole power of 2 between 4 and 1024.");

        }
    }
}
