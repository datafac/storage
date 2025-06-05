using DataFac.Memory;
using DTOMaker.Models;
using System;
namespace MyOrg.Models
{
    [Entity]
    [Id(1)]
    public interface ITree<TK, TV>
    {
        [Member(1)] int Count { get; }
        [Member(2)] TK Key { get; }
        [Member(3)] TV Value { get; }
        [Member(4)] ITree<TK, TV>? Left { get; }
        [Member(5)] ITree<TK, TV>? Right { get; }
    }
    [Entity]
    [Id(2)]
    public interface IMyTree : ITree<String, Octets>
    {
    }
}
