using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.CSPoco.Tests
{
    public class RecursiveGraphTests
    {
        private readonly string models =
            """
            using System;
            using DTOMaker.Models;
            using DTOMaker.Models.MemBlocks;
            using DTOMaker.Models.MessagePack;
            namespace MyOrg.Models
            {
                [Entity][Id(1)]
                public interface INode
                {
                    [Member(1)] String Key { get; set; }
                }

                [Entity][Id(2)]
                public interface IStringNode : INode
                {
                    [Member(1)] String Value { get; set; }
                }

                [Entity][Id(3)]
                public interface INumericNode : INode
                {
                }

                [Entity][Id(5)]
                public interface IInt64Node : INumericNode
                {
                    [Member(1)] Int64 Value { get; set; }
                }

                [Entity][Id(6)]
                public interface IDoubleNode : INumericNode
                {
                    [Member(1)] Double Value { get; set; }
                }

                [Entity][Id(4)]
                public interface IBooleanNode : INode
                {
                    [Member(1)] Boolean Value { get; set; }
                }

                [Entity][Id(7)]
                public interface ITree
                {
                    [Member(1)] ITree? Left { get; set; }
                    [Member(2)] ITree? Right { get; set; }
                    [Member(3)] INode? Node { get; set; }
                }
            }
            
            """;

        [Fact]
        public void RecursiveGraph00_GeneratedSourcesLengthShouldBe7()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("MyOrg.Models.BooleanNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[1].HintName.ShouldBe("MyOrg.Models.DoubleNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[2].HintName.ShouldBe("MyOrg.Models.Int64Node.CSPoco.g.cs");
            generatorResult.GeneratedSources[3].HintName.ShouldBe("MyOrg.Models.Node.CSPoco.g.cs");
            generatorResult.GeneratedSources[4].HintName.ShouldBe("MyOrg.Models.NumericNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[5].HintName.ShouldBe("MyOrg.Models.StringNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[6].HintName.ShouldBe("MyOrg.Models.Tree.CSPoco.g.cs");
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource0()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource1()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[1];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource2()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[2];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource3()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[3];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource4()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[4];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource5()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[5];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task RecursiveGraph01_VerifyGeneratedSource6()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(models, LanguageVersion.LatestMajor);

            // custom generation checks
            var source = generatorResult.GeneratedSources[6];
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

    }
}