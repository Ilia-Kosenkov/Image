using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Image
{
    public class Image<T> : IImmutableImage<T> where T : unmanaged
    {
        private readonly T[] _data;

        public static IImmutableList<Type> AllowedTypes { get; } =
        new[] 
            {
                typeof(double),
                typeof(float),
                typeof(byte),
                typeof(ushort),
                typeof(uint),
                typeof(sbyte),
                typeof(short),
                typeof(int),
            }.ToImmutableList();

        public int Height { get; }
        public int Width { get; }

        public T this[int i, int j] => _data[i * Width + j];

        public Image(ReadOnlySpan<T> data, int width, int height)
        {
           ThrowIfTypeMismatch();

            if(width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            // Size mismatch
            if (data.Length < width * height)
                throw new ArgumentException();

            _data = new T[width * height];
            data.CopyTo(_data);
            Width = width;
            Height = height;
        }

        public unsafe Image(ReadOnlySpan<byte> byteData, int width, int height)
        {
            ThrowIfTypeMismatch();

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var size = Unsafe.SizeOf<T>();

            // Size mismatch
            if (size * byteData.Length < width * height)
                throw new ArgumentException();

            _data = new T[width * height];
            
            fixed (void* dataPtr = &_data[0])
                fixed(void* srcPtr = byteData)
                    Unsafe.CopyBlock(dataPtr, srcPtr, (uint)byteData.Length);
            
        }

        
       

        public T Max()
        {
            throw new NotImplementedException();
        }

        public T Min()
        {
            throw new NotImplementedException();
        }

        public double Percentile(T lvl)
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<T> Copy()
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<T> Transpose()
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<TOther> CastTo<TOther>() where TOther : unmanaged
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<TOther> CastTo<TOther>(Func<T, TOther> caster) where TOther : unmanaged
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<T> Clamp(T low, T high)
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<T> Scale(T low, T high)
        {
            throw new NotImplementedException();
        }

        public bool Equals(OldImage other)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        private static void ThrowIfTypeMismatch()
        {
            if (!AllowedTypes.Contains(typeof(T)))
                throw new NotSupportedException(typeof(T).ToString());
        }
    }
}