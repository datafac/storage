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
    public class MultiDomainTests
    {
        private readonly string inputSource =
            """
                using DTOMaker.Models;
                namespace MyOrg.DomainA
                {
                    [Entity][Id(1)] public interface IMyDTO { }
                }
                namespace MyOrg.DomainB
                {
                    [Entity][Id(2)] public interface IMyDTO { }
                }
                """;

        [Fact]
        public void Domains01_2Entities_Generates3Outputs()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(2);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("MyOrg.DomainA.MyDTO.CSRecord.g.cs");
            generatorResult.GeneratedSources[1].HintName.ShouldBe("MyOrg.DomainB.MyDTO.CSRecord.g.cs");
        }

        [Fact]
        public async Task Domains03_2Entities_VerifyDomainA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.ShouldBe(2);
            var source = generatorResult.GeneratedSources[0];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Domains04_2Entities_VerifyDomainB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.ShouldBe(2);
            var source = generatorResult.GeneratedSources[1];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        private readonly string inputSource2 =
            """
                using DTOMaker.Models;
                using DTOMaker.Models.MessagePack;
                namespace MyOrg.DomainA
                {
                    [Entity][Id(2)] public interface IMyBase { }
                }
                namespace MyOrg.DomainB
                {
                    [Entity][Id(1)] public interface IMyDTO : MyOrg.DomainA.IMyBase { }
                }
                """;

        [Fact]
        public async Task Domains05_BaseInOtherNamespaceA()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource2, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.ShouldBe(2);
            var source = generatorResult.GeneratedSources[0];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Domains06_BaseInOtherNamespaceB()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource2, LanguageVersion.LatestMajor);

            generatorResult.GeneratedSources.Length.ShouldBe(2);
            var source = generatorResult.GeneratedSources[1];

            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}