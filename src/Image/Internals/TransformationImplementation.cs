#nullable enable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImageCore.Internals
{
    internal static class ImageTransImpl
    {
        public delegate int TargetSelector(int i, int j, int height, int width);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int TransposeSelector(int i, int j, int height, int width)
            => j * height + i;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotationSelector0(int i, int j, int height, int width)
            => i * width + j;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotationSelector90(int i, int j, int height, int width)
            => (width - 1 - j) * height + i;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotationSelector180(int i, int j, int height, int width)
            => width * height - 1 - (i * width + j);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotationSelector270(int i, int j, int height, int width)
            => j * height + (height - i - 1);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FlippingHorizontallySelector(int i, int j, int height, int width)
            => i * width + (width - 1 - j);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FlippingVerticallySelector(int i, int j, int height, int width)
            => (height - 1 - i) * width + j;

        public static TargetSelector GetTransposeSelector() => TransposeSelector;

        public static TargetSelector GetRotationSelector(RotationDegree degree)
            => degree switch
            {
                RotationDegree.Zero => RotationSelector0,
                RotationDegree.Rotate90 => RotationSelector90,
                RotationDegree.Rotate180 => RotationSelector180,
                RotationDegree.Rotate270 => RotationSelector270,
                _ => throw new InvalidEnumArgumentException(nameof(degree), (int) degree, typeof(RotationDegree))
            };

        public static TargetSelector GetFlipSelector(FlipDirection direction)
            => direction switch
            {
                FlipDirection.Horizontally => FlippingHorizontallySelector,
                FlipDirection.Vertically => FlippingVerticallySelector,
                _ => throw new InvalidEnumArgumentException(nameof(direction), (int) direction, typeof(FlipDirection))
            };  

        public static void Transform<T>(ReadOnlySpan<T> source, Span<T> target, int height, int width,
            TargetSelector targetSelector)
        {
            _ = targetSelector ??
                throw new ArgumentNullException(nameof(targetSelector));

            if (target.Length < source.Length)
                throw new ArgumentException(nameof(target));
            if (height < 0)
                throw new ArgumentException(nameof(height));
            if (width < 0)
                throw new ArgumentException(nameof(width));
            if (target.Length < height * width)
                throw new ArgumentException(nameof(target));

            for (var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
                target[targetSelector(i, j, height, width)] = source[i * width + j];
        }

    }
}
