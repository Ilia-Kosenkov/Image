using System;

namespace ImageCore.Internals
{
    internal static class RotationImplementation
    {
        public static void Rotate90<T>(ReadOnlySpan<T> source, Span<T> target, int height, int width)
        {
            if (target.Length < source.Length)
                throw new ArgumentException(nameof(target));
            if (height < 0)
                throw new ArgumentException(nameof(height));
            if (width < 0)
                throw new ArgumentException(nameof(width));
            if (target.Length < height * width)
                throw new ArgumentException(nameof(target));

            for(var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
                target[j * height + i] = source[i * width + j];
        }
    }
}
