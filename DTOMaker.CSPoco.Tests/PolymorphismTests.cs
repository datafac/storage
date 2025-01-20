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
    public class PolymorphismTests
    {
        private readonly string inputSource =
            """
                using DTOMaker.Models;
                namespace MyOrg.Models
                {
                    [Entity] [Id("Polygon")]
                    public interface IPolygon { }

                    [Entity] [Id("Triangle")] 
                    public interface ITriangle : IPolygon { }

                    [Entity] [Id("Equilateral")] 
                    public interface IEquilateral : ITriangle
                    {
                        [Member(1)] double Length { get; set; }
                    }

                    [Entity] [Id("RightTriangle")] 
                    public interface IRightTriangle : ITriangle
                    {
                        [Member(1)] double Length { get; set; }
                        [Member(2)] double Height { get; set; }
                    }

                    [Entity] [Id("Quadrilateral")] 
                    public interface IQuadrilateral : IPolygon { }

                    [Entity] [Id("Square")] 
                    public interface ISquare : IQuadrilateral
                    {
                        [Member(1)] double Length { get; set; }
                    }

                    [Entity] [Id("Rectangle")] 
                    public interface IRectangle : IQuadrilateral
                    {
                        [Member(1)] double Length { get; set; }
                        [Member(2)] double Height { get; set; }
                    }
                }
                """;

        [Fact]
        public async Task Polymorphic02_Equilateral()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[0];
            source.HintName.Should().Be("MyOrg.Models.Equilateral.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic03_Polygon()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[1];
            source.HintName.Should().Be("MyOrg.Models.Polygon.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic04_Quadrilateral()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[2];
            source.HintName.Should().Be("MyOrg.Models.Quadrilateral.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic05_Rectangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[3];
            source.HintName.Should().Be("MyOrg.Models.Rectangle.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic06_RightTriangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[4];
            source.HintName.Should().Be("MyOrg.Models.RightTriangle.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic07_Square()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[5];
            source.HintName.Should().Be("MyOrg.Models.Square.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic08_Triangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.Should().BeNull();
            generatorResult.Diagnostics.Should().BeEmpty();
            generatorResult.GeneratedSources.Length.Should().Be(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[6];
            source.HintName.Should().Be("MyOrg.Models.Triangle.CSPoco.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}