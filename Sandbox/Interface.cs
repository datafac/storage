using DataFac.Memory;
namespace MyOrg.Models
{
    public interface IMyDTO
    {
        Octets  Field1 { get; set; }
        Octets? Field2 { get; set; }
    }
}
