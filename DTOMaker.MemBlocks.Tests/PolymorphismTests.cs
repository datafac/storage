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
    public class PolymorphismTests
    {
        private readonly string inputSource =
            """
                using DTOMaker.Models;
                using DTOMaker.Models.MemBlocks;
                namespace MyOrg.Models
                {
                    [Entity][Layout(LayoutMethod.Linear)]
                    [Id(1)]
                    public interface IPolygon { }

                    [Entity][Layout(LayoutMethod.Linear)] 
                    [Id(2)]
                    public interface ITriangle : IPolygon { }

                    [Entity][Layout(LayoutMethod.Linear)] 
                    [Id(3)]
                    public interface IEquilateral : ITriangle
                    {
                        [Member(1)] double Length { get; set; }
                    }

                    [Entity][Layout(LayoutMethod.Linear)] 
                    [Id(4)]
                    public interface IRightTriangle : ITriangle
                    {
                        [Member(1)] double Length { get; set; }
                        [Member(2)] double Height { get; set; }
                    }

                    [Entity][Layout(LayoutMethod.Linear)] 
                    [Id(5)]
                    public interface IQuadrilateral : IPolygon { }

                    [Entity][Layout(LayoutMethod.Linear)] 
                    [Id(6)]
                    public interface ISquare : IQuadrilateral
                    {
                        [Member(1)] double Length { get; set; }
                    }

                    [Entity][Layout(LayoutMethod.Linear)] 
                    [Id(7)]
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
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[0];
            source.HintName.ShouldBe("MyOrg.Models.Equilateral.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic03_Polygon()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[1];
            source.HintName.ShouldBe("MyOrg.Models.Polygon.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic04_Quadrilateral()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[2];
            source.HintName.ShouldBe("MyOrg.Models.Quadrilateral.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic05_Rectangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[3];
            source.HintName.ShouldBe("MyOrg.Models.Rectangle.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic06_RightTriangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[4];
            source.HintName.ShouldBe("MyOrg.Models.RightTriangle.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic07_Square()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[5];
            source.HintName.ShouldBe("MyOrg.Models.Square.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task Polymorphic08_Triangle()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(inputSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.ShouldBeEmpty();
            generatorResult.GeneratedSources.Length.ShouldBe(7);
            GeneratedSourceResult source = generatorResult.GeneratedSources[6];
            source.HintName.ShouldBe("MyOrg.Models.Triangle.MemBlocks.g.cs");
            string outputCode = string.Join(Environment.NewLine, source.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}