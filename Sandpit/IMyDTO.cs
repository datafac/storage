using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DTOMaker.Models;
namespace MyOrg.Models
{
    public enum Kind16 : ushort
    {
        Undefined,
        Kind1,
        MaxKind = ushort.MaxValue,
    }
    [Entity]
    [EntityLayout(LayoutMethod.SequentialV1)]
    public interface IMyDTO
    {
        [Member(1)]
        IList<Int16?>? Field1 { get; set; }
    }
}
