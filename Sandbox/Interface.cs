using DataFac.Memory;
using DTOMaker.Models;
namespace MyOrg.Models
{
    [Entity]
    public interface IMyDTO
    {
        [Member(1)] Octets Field1 { get; set; }
    }
}
