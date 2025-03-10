using System;

namespace DTOMaker.Gentime
{
    public readonly struct Union<T0, T1> where T0 : struct where T1 : struct
    {
        public readonly int Index;
        public readonly T0 Value0;
        public readonly T1 Value1;

        public Union(T0 value0) : this()
        {
            Index = 0;
            Value0 = value0;
            Value1 = default;
        }

        public Union(T1 value1) : this()
        {
            Index = 1;
            Value0 = default;
            Value1 = value1;
        }

        public bool TryPick0(out T0 value0, out T1 value1)
        {
            value0 = Value0;
            value1 = Value1;
            return Index == 0;
        }

        public void Switch(Action<T0> action0, Action<T1> action1)
        {
            if (Index == 0)
                action0(Value0);
            else
                action1(Value1);
        }

        public TResult Match<TResult>(Func<T0, TResult> action0, Func<T1, TResult> action1)
        {
            return Index == 0
                ? action0(Value0)
                : action1(Value1);
        }
    }
}
