using Shouldly;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.MessagePack.Tests
{

    public class InheritanceTests
    {
        [Fact]
        public async Task Entity02_VerifyCommon()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id(1)]
                    public interface IMyBase
                    {
                    }
                    [Entity]
                    [Id(2)]
                    [MemberKeyOffset(10)]
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
            source.HintName.ShouldBe("MyOrg.Models.MyBase.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Entity03_VerifySpecific()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id(1)]
                    public interface IMyBase
                    {
                    }
                    [Entity]
                    [Id(2)]
                    [MemberKeyOffset(10)]
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
            source.HintName.ShouldBe("MyOrg.Models.MyDTO.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Member01_MemberKeysAreUniqueWithinTree()
        {
            var inputSource =
                """
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.Models
                {
                    [Entity]
                    [Id(1)]
                    public interface IMyBase
                    {
                        [Member(1)] double BaseField1 { get; set; }
                    }
                    [Entity]
                    [Id(2)]
                    [MemberKeyOffset(10)]
                    public interface IMyDTO : IMyBase
                    {
                        [Member(1)] double DTOField1 { get; set; }
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
            source.HintName.ShouldBe("MyOrg.Models.MyDTO.MessagePack.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}