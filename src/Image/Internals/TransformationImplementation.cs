using System;

namespace ImageCore.Internals
{
    internal static class TransformationImplementation
    {
        public static void Transpose<T>(ReadOnlySpan<T> source, Span<T> target, int height, int width)
        {
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
                target[j * height + i] = source[i * width + j];
        }

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

            for (var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
                target[(width - 1 - j) * height + i] = source[i * width + j];
        }

        public static void Rotate180<T>(ReadOnlySpan<T> source, Span<T> target, int height, int width)
        {
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
                target[width * height - 1 - (i * width + j)] = source[i * width + j];
        }

        public static void Rotate270<T>(ReadOnlySpan<T> source, Span<T> target, int height, int width)
        {
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
                target[j * height + (height - i - 1)] = source[i * width + j];
        }
    }
}
