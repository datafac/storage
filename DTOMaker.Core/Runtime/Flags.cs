namespace DTOMaker.Runtime
{
    public readonly struct Flags
    {
        public static Flags Null = new Flags(false);
        public static Flags NonNull = new Flags(true);

        private const byte Flag_IsAssigned = 0b_1000_0000;
        private const byte Flag_HasValue   = 0b_0000_0001;
        //private const byte Flag_IsArray    = 0b_0000_0010;

        private readonly byte _flags;

        public Flags() { _flags = 0; }
        public Flags(byte flags) { _flags = flags; }
        public Flags(bool hasValue)
        {
            _flags = Flag_IsAssigned;
            _flags |= (hasValue ? Flag_HasValue : (byte)0);
            //_flags |= (isArray ? Flag_IsArray : (byte)0);
        }

        public byte AsByte() => _flags;

        public bool IsAssigned => (_flags & Flag_IsAssigned) != 0;
        public bool HasValue => (_flags & Flag_HasValue) != 0;
        public bool IsNull => (_flags & Flag_HasValue) == 0;
        //public bool IsArray => (_flags & Flag_IsArray) != 0;
    }
}
