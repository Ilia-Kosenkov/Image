using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MathNet;
using MathNet.Numerics;

namespace Image.Internal
{
    public static class Numerics
    {
        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern T DangerousAdd<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern T DangerousSubtract<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern T DangerousMultiply<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern T DangerousDivide<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern T DangerousNegate<T>(T item) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern bool DangerousEquals<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern bool DangerousNotEquals<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern bool DangerousGreaterThan<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern bool DangerousLessThan<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern bool DangerousGreaterEquals<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern bool DangerousLessEquals<T>(T left, T right) where T : unmanaged;

        [MethodImpl(MethodImplOptions.ForwardRef, MethodCodeType = MethodCodeType.IL)]
        public static extern int Compare<T>(T left, T right) where T : unmanaged;

        public static int Test<U>() where U : unmanaged
        {
            var y = DangerousEquals(default(U), default(U));
            var x = 1.0f.AlmostEqual(2.0f);
            double f = 4.GetHashCode();
            
            var hh = f > 10;
            throw new ArithmeticException();
            return (typeof(U) == typeof(double)) ? 1 : 0;
        }
    }
}
