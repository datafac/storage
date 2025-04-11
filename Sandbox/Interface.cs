using DataFac.Memory;
using System;
namespace MyOrg.Models
{
    public interface IOther
    {
        Int64 Value1 { get; }
        Int64 Value2 { get; }
    }
    public interface IMyDTO
    {
        IOther? Other1 { get; }
        Octets Field1 { get; }
        Octets? Field2 { get; }
    }
}
