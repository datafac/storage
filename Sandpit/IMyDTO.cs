using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandpit
{
    public interface IMyDTO
    {
        bool Field1 { get; set; }
        sbyte Field2 { get; set; }
        byte Field3 { get; set; }
        short Field4 { get; set; }
        ushort Field5 { get; set; }
        char Field6 { get; set; }
        Half Field7 { get; set; }
        int Field8 { get; set; }
        uint Field9 { get; set; }
        float Field10 { get; set; }
        long Field11 { get; set; }
        ulong Field12 { get; set; }
        double Field13 { get; set; }
        Guid Field14 { get; set; }
        Int128 Field15 { get; set; }
        UInt128 Field16 { get; set; }
        Decimal Field17 { get; set; }
    }
}
