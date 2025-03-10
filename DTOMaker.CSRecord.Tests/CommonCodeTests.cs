using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.CSRecord.Tests
{
    public class CommonCodeTests
    {
        [Fact]
        public async Task Common02_EntityBaseB()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity][Id(2)]
                    public interface IMyBase
                    {
                    }
                    [Entity][Id(1)]
                    public interface IMyDTO : IMyBase
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(2);
            GeneratedSourceResult source = generatorResult.GeneratedSources[0];

            // custom generation checks
            source.HintName.ShouldBe("MyOrg.Models.MyBase.CSRecord.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Common03_EntityBaseC()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity][Id(2)]
                    public interface IMyBase
                    {
                    }
                    [Entity][Id(1)]
                    public interface IMyDTO : IMyBase
                    {
                    }
                }
                """;

            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(2);
            GeneratedSourceResult source = generatorResult.GeneratedSources[1];

            // custom generation checks
            source.HintName.ShouldBe("MyOrg.Models.MyDTO.CSRecord.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}