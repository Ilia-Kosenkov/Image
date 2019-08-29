using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Image
{
    public abstract class Image 
    {
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
    }

    public class Image<T> : Image, IImmutableImage<T> where T 
        : unmanaged, IComparable<T>
    {
        private static readonly Func<T, T, T> Add;
        private static readonly Func<T, T, T> Subtract;
        private static readonly Func<T, T> Invert;
        private static readonly Func<T, T, T> Multiply;
        private static readonly Func<T, T, T> Divide;

        private readonly T[] _data;
        private T? _max;
        private T? _min;
        
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

        public Image(ReadOnlySpan<byte> byteData, int width, int height)
        {
            ThrowIfTypeMismatch();

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var size = Unsafe.SizeOf<T>();

            // Size mismatch
            if (size * byteData.Length < width * height)
                throw new ArgumentException();

            _data = new T[width * height];


            if (!MemoryMarshal.Cast<byte, T>(byteData).TryCopyTo(_data))
                throw new InvalidOperationException();

            Width = width;
            Height = height;
        }

      

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Max()
        {
            if (_max is null)
            {
                var temp = default(T);
                foreach (var item in _data)
                    if (item.CompareTo(temp) >= 0)
                        temp = item;

                _max = temp;
            }

            return _max.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Min()
        {
            if (_min is null)
            {
                var temp = default(T);
                foreach (var item in _data)
                    if (item.CompareTo(temp) <= 0)
                        temp = item;

                _min = temp;
            }

            return _min.Value;
        }

        public double Percentile(T lvl)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> GetByteView()
            => MemoryMarshal.Cast<T, byte>(new ReadOnlySpan<T>(_data));

        public IImmutableImage<T> Copy()
            => new Image<T>(_data, Width, Height);

        public IImmutableImage<T> Transpose()
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<TOther> CastTo<TOther>() where TOther : unmanaged, IComparable<TOther>
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<TOther> CastTo<TOther>(Func<T, TOther> caster) where TOther : unmanaged, IComparable<TOther>
        {
            throw new NotImplementedException();
        }

        public IImmutableImage<T> Clamp(T low, T high)
        {
            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);
                _data.AsSpan().CopyTo(span);

                foreach (ref var item in span)
                    if (item.CompareTo(low) < 0)
                        item = low;
                    else if (item.CompareTo(high) > 0)
                        item = high;

                return new Image<T>(span, Width, Height);
            }
        }

        public IImmutableImage<T> Scale(T low, T high)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IImmutableImage<T> other)
        {
            if (other is null || Width != other.Width || Height != other.Height)
                return false;

            // WATCH: Possible optimization
            return GetByteView().SequenceEqual(other.GetByteView());
        }

        public bool Equals(IImmutableImage other)
            => other is IImmutableImage<T> img
               && Equals(img);
        


        public object Clone() => Copy();


        double IImmutableImage.Min() => (double)Convert.ChangeType(Min(), typeof(double));
        double IImmutableImage.Max() => (double)Convert.ChangeType(Max(), typeof(double));
        IImmutableImage IImmutableImage.Clamp(double low, double high)
            => Clamp((T) Convert.ChangeType(low, typeof(T)), (T) Convert.ChangeType(high, typeof(T)));

        public override bool Equals(object obj)
            => obj is IImmutableImage<T> other
               && Equals(other);

        // TODO : Fix poor hash function
        public override int GetHashCode()
            => _data.GetHashCode() ^ ((Width << 16 ) ^ Height);

        static Image()
        {
        }

        private static void ThrowIfTypeMismatch()
        {
            if (!AllowedTypes.Contains(typeof(T)))
                throw new NotSupportedException(typeof(T).ToString());
        }


    }
}