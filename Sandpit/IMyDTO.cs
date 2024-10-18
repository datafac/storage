using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DTOMaker.Models;
namespace MyOrg.Models
{
    public enum Kind32 : int
    {
        Default,
        Kind1 = 1,
        MaxKind = int.MaxValue,
    }
    [Entity]
    [EntityLayout(LayoutMethod.SequentialV1)]
    public interface IMyDTO
    {
        [Member(1)]
        Kind32 Field1 { get; set; }
    }
}
