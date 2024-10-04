using System;
using System.Runtime.InteropServices;

namespace DTOMaker.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct Convert32
    {
        [FieldOffset(0)] public readonly Int32 Int32Value;
        [FieldOffset(0)] public readonly float FloatValue;

        public Convert32(float floatValue)
        {
            Int32Value = default;
            FloatValue = floatValue;
        }

        public Convert32(int int32Value)
        {
            FloatValue = default;
            Int32Value = int32Value;
        }
    }

}
