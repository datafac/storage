using DataFac.Memory;
using System;
namespace MyOrg.Models
{
    public interface IOther
    {
        Int64 Value1 { get; set; }
        Int64 Value2 { get; set; }
    }
    public interface IMyDTO
    {
        IOther? Other1 { get; set; }
        Octets Field1 { get; set; }
        Octets? Field2 { get; set; }
    }
}
