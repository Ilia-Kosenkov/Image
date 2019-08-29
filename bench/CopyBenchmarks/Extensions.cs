using System;
using System.Runtime.CompilerServices;

namespace CopyBenchmarks
{
    internal static class Extensions
    {
        public static unsafe Span<TOther> TypeCast<T, TOther>(this Span<T> span) where T : unmanaged where TOther : unmanaged
        {
            Span<TOther> other;
            fixed (T* ptr = span)
                other = new Span<TOther>((TOther*) ptr, span.Length * Unsafe.SizeOf<T>());

            return other;
        }


        public static unsafe Span<TOther> TypeCast2<T, TOther>(this Span<T> span) where T : unmanaged where TOther : unmanaged
        {
            return new Span<TOther>(Unsafe.AsPointer(ref span[0]), span.Length * Unsafe.SizeOf<T>());
        }
    }
}