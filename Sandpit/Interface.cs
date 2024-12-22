using DTOMaker.Models;
using DTOMaker.Models.MemBlocks;
using DTOMaker.Models.MessagePack;

namespace Sandpit
{
    [Entity][EntityTag(1)]
    [Id("Polygon")][Layout(LayoutMethod.SequentialV1)] 
    public interface IPolygon { }

    [Entity][EntityTag(2)]
    [Id("Triangle")][Layout(LayoutMethod.SequentialV1)]
    public interface ITriangle : IPolygon { }

    [Entity][EntityTag(3)]
    [Id("Equilateral")][Layout(LayoutMethod.SequentialV1)]
    public interface IEquilateral : ITriangle
    {
        [Member(1)] double Length { get; set; }
    }

    [Entity]
    [EntityTag(4)]
    [Id("RightTriangle")][Layout(LayoutMethod.SequentialV1)]
    public interface IRightTriangle : ITriangle
    {
        [Member(1)] double Length { get; set; }
        [Member(2)] double Height { get; set; }
    }

    [Entity][EntityTag(5)]
    [Id("Quadrilateral")][Layout(LayoutMethod.SequentialV1)]
    public interface IQuadrilateral : IPolygon { }

    [Entity]
    [EntityTag(6)]
    [Id("Square")][Layout(LayoutMethod.SequentialV1)]
    public interface ISquare : IQuadrilateral
    {
        [Member(1)] double Length { get; set; }
    }

    [Entity]
    [EntityTag(7)]
    [Id("Rectangle")][Layout(LayoutMethod.SequentialV1)]
    public interface IRectangle : IQuadrilateral
    {
        [Member(1)] double Length { get; set; }
        [Member(2)] double Height { get; set; }
    }
}
