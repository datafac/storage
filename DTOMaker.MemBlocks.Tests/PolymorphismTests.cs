using FluentAssertions;
using Microsoft.CodeAnalysis;
using System.Linq;
using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Microsoft.CodeAnalysis.CSharp;

namespace DTOMaker.MemBlocks.Tests
{
    public class PolymorphismTests
    {
        private readonly string inputSource =
            """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity] [EntityLayout("Polygon", LayoutMethod.SequentialV1)]
                    public interface IPolygon { }

                    [Entity] [EntityLayout("Triangle", LayoutMethod.SequentialV1)] 
                    public interface ITriangle : IPolygon { }

                    [Entity] [EntityLayout("Equilateral", LayoutMethod.SequentialV1)] 
                    public interface IEquilateral : ITriangle
                    {
                        [Member(1)] double Length { get; set; }
                    }

                    [Entity] [EntityLayout("RightTriangle", LayoutMethod.SequentialV1)] 
                    public interface IRightTriangle : ITriangle
                    {
                        [Member(1)] double Length { get; set; }
                        [Member(2)] double Height { get; set; }
                    }

                    [Entity] [EntityLayout("Quadrilateral", LayoutMethod.SequentialV1)] 
                    public interface IQuadrilateral : IPolygon { }

                    [Entity] [EntityLayout("Square", LayoutMethod.SequentialV1)] 
                    public interface ISquare : IQuadrilateral
                    {
                        [Member(1)] double Length { get; set; }
                    }

                    [Entity] [EntityLayout("Rectangle", LayoutMethod.SequentialV1)] 
                    public interface IRectangle : IQuadrilateral
                    {
                        [Member(1)] double Length { get; set; }
                        [Member(2)] double Height { get; set; }
                    }
                }
                """;

        [Fact]
        public async Task Polymorphic01_EntityBase()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[0];
            source.HintName.Should().Be("MyOrg.Models.EntityBase.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic02_Equilateral()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[1];
            source.HintName.Should().Be("MyOrg.Models.Equilateral.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic03_Polygon()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[2];
            source.HintName.Should().Be("MyOrg.Models.Polygon.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic04_Quadrilateral()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[3];
            source.HintName.Should().Be("MyOrg.Models.Quadrilateral.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic05_Rectangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[4];
            source.HintName.Should().Be("MyOrg.Models.Rectangle.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic06_RightTriangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[5];
            source.HintName.Should().Be("MyOrg.Models.RightTriangle.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic07_Square()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[6];
            source.HintName.Should().Be("MyOrg.Models.Square.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic08_Triangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(8);
            GeneratedSourceResult source = generatorResult.GeneratedSources[7];
            source.HintName.Should().Be("MyOrg.Models.Triangle.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}