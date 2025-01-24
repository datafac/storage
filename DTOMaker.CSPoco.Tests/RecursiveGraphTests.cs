using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                [Entity]
                public interface INode
                {
                    [Member(1)] String Key { get; set; }
                }

                [Entity]
                public interface IStringNode : INode
                {
                    [Member(1)] String Value { get; set; }
                }

                [Entity]
                public interface INumericNode : INode
                {
                }

                [Entity]
                public interface IInt64Node : INumericNode
                {
                    [Member(1)] Int64 Value { get; set; }
                }

                [Entity]
                public interface IDoubleNode : INumericNode
                {
                    [Member(1)] Double Value { get; set; }
                }

                [Entity]
                public interface IBooleanNode : INode
                {
                    [Member(1)] Boolean Value { get; set; }
                }

                [Entity]
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
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Should().BeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.Should().Be(7);
            generatorResult.GeneratedSources[0].HintName.Should().Be("MyOrg.Models.BooleanNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[1].HintName.Should().Be("MyOrg.Models.DoubleNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[2].HintName.Should().Be("MyOrg.Models.Int64Node.CSPoco.g.cs");
            generatorResult.GeneratedSources[3].HintName.Should().Be("MyOrg.Models.Node.CSPoco.g.cs");
            generatorResult.GeneratedSources[4].HintName.Should().Be("MyOrg.Models.NumericNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[5].HintName.Should().Be("MyOrg.Models.StringNode.CSPoco.g.cs");
            generatorResult.GeneratedSources[6].HintName.Should().Be("MyOrg.Models.Tree.CSPoco.g.cs");
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