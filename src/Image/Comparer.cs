using System;
using System.Collections.Generic;

namespace ImageCore
{
    internal class Comparer<T> : IComparer<T>
        where T : unmanaged, IComparable<T>
    {

        public static IComparer<T> Default { get; } = new Comparer<T>();
            
        public int Compare(T x, T y)
        {
#if ALLOW_UNSAFE_IL_MATH
            return Internal.Numerics.MathOps.DangerousCompare(x, y);
#else
            return System.Collections.Generic.Comparer<T>.Default.Compare(x, y);
#endif
        }
    }
}
