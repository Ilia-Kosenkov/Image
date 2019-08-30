using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if ALLOW_UNSAFE_IL_MATH
using static Internal.Numerics.MathOps;
#endif

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

    public class Image<T> : Image, IImmutableImage<T> 
        where T : unmanaged, IComparable<T>
    {
 
        private readonly T[] _data;
        private T? _max;
        private T? _min;
        
        public int Height { get; }
        public int Width { get; }

        public T this[int i, int j] =>
            i < 0 || i >= Height
                ? throw new ArgumentOutOfRangeException(nameof(i))
                : j < 0 || j >= Width
                    ? throw new ArgumentOutOfRangeException(nameof(j))
                    : _data[i * Width + j];

        public Image(ReadOnlySpan<T> data, int height, int width)
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

        public Image(ReadOnlySpan<byte> byteData, int height, int width)
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
                var temp = _data[0];
                foreach (var item in _data)
#if ALLOW_UNSAFE_IL_MATH
                    if (DangerousGreaterEquals(item, temp))
#else
                    if (item.CompareTo(temp) >= 0)
#endif
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
                var temp = _data[0];
                foreach (var item in _data)
#if ALLOW_UNSAFE_IL_MATH
                    if (DangerousLessEquals(item, temp))
#else
                    if (item.CompareTo(temp) <= 0)
#endif

                        temp = item;

                _min = temp;
            }

            return _min.Value;
        }

        public T Percentile(T lvl)
        {
#if ALLOW_UNSAFE_IL_MATH
            var hund = DangerousCast<int, T>(100);
            var zero = DangerousCast<int, T>(0);
            if (DangerousLessThan(lvl, zero)
                || DangerousGreaterThan(lvl, hund))
                throw new ArgumentOutOfRangeException(nameof(lvl));

            if (DangerousEquals(lvl, zero))
                return Min();
            if (DangerousEquals(lvl, hund))
                return Max();

            // ceil(lvl * width * height / 100)
            var len = (int) Math.Ceiling(
                DangerousCast<T, double>(
                        DangerousMultiply(
                            lvl, 
                            DangerousCast<int, T>(Width * Height))) / 100.0);

            if (len < 1)
                len = 1;

            return _data.OrderBy(x => x, Comparer<T>.Default).Skip(len - 1).First();
            
#else
            dynamic l = lvl;
            if (Math.Abs(l) < double.Epsilon)
                return Min();
            if (Math.Abs(l - 1) < double.Epsilon)
                return Max();
            var query = _data.OrderBy(x => x, Comparer<T>.Default);

            var len = (int)Math.Ceiling(l * Width * Height / 100.0);

            if (len < 1)
                len = 1;

            return query.Skip(len - 1).Take(1).First();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetByteView()
            => MemoryMarshal.Cast<T, byte>(new ReadOnlySpan<T>(_data));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetView() => _data;

        public IImmutableImage<T> Copy()
            => new Image<T>(_data, Height, Width);

        public IImmutableImage<T> Transpose()
        {
            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);
                
                for(var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                    span[j * Height + i] = _data[i * Width + j];

                return  new Image<T>(span, Width, Height);
            }
        }

        public IImmutableImage<TOther> CastTo<TOther>() where TOther 
            : unmanaged, IComparable<TOther>
        {
            using (var pool = MemoryPool<TOther>.Shared.Rent(Width * Height))
            {
                var span = pool.Memory.Span.Slice(Width * Height);
                for (var i = 0; i < _data.Length; i++)
#if ALLOW_UNSAFE_IL_MATH
                    span[i] = DangerousCast<T, TOther>(_data[i]);
#else
                    span[i] = (TOther) Convert.ChangeType(_data[i], typeof(TOther));
#endif
                return new Image<TOther>(span, Height, Width);
            }
        }

        public IImmutableImage<TOther> CastTo<TOther>(Func<T, TOther> caster) where TOther : unmanaged, IComparable<TOther>
        {
            using (var pool = MemoryPool<TOther>.Shared.Rent(Width * Height))
            {
                var span = pool.Memory.Span.Slice(Width * Height);
                for (var i = 0; i < _data.Length; i++)
                    span[i] = caster(_data[i]);

                return new Image<TOther>(span, Height, Width);
            }
        }

        public IImmutableImage<T> Clamp(T low, T high)
        {
            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);
                _data.AsSpan().CopyTo(span);

                foreach (ref var item in span)
#if ALLOW_UNSAFE_IL_MATH
                    if (DangerousLessThan(item, low))
                        item = low;
                    else if(DangerousGreaterThan(item ,high))
                        item = high;
#else
                    if (item.CompareTo(low) < 0)
                        item = low;
                    else if (item.CompareTo(high) > 0)
                        item = high;
#endif
                return new Image<T>(span, Height, Width);
            }
        }

        public IImmutableImage<T> Scale(T low, T high)
        {
#if !ALLOW_UNSAFE_IL_MATH
            dynamic dLow = low;
            dynamic dHigh = high;
            dynamic min = Min();
            dynamic max = Max();
            var enumer = dHigh - dLow;
            var denomer = max - min;

            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);

                if (denomer == 0)
                {
                    var filler = (T) ((dLow + dHigh) / 2);
                    span.Fill(filler);
                }
                else
                    for (var i = 0; i < _data.Length; i++)
                        span[i] = (T) (dLow + (_data[i] - min) * enumer / denomer);
                return new Image<T>(span, Height, Width);
            }
#else

            var min = Min();
            var max = Max();
            var enumer = DangerousSubtract(high, low);
            var denomer = DangerousSubtract(max, min);

            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);

                if (DangerousEquals(denomer, default))
                {
                    var filler = 
                        DangerousDivide(
                            DangerousAdd(low, high), 
                            DangerousCast<int, T>(2));
                    span.Fill(filler);
                }
                else
                    for (var i = 0; i < _data.Length; i++)
                        span[i] = 
                            DangerousAdd(
                            DangerousDivide(
                                DangerousMultiply(
                                    DangerousSubtract(_data[i], min), 
                                    enumer), 
                                denomer), 
                            low);

                return new Image<T>(span, Height, Width);
            }
#endif
        }

        public IImmutableImage<T> AddScalar(T item)
        {
#if !ALLOW_UNSAFE_IL_MATH
            dynamic temp = item;
#endif

            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);

                for (var i = 0; i < _data.Length; i++)
#if ALLOW_UNSAFE_IL_MATH
                    span[i] = DangerousAdd(_data[i], item);
#else
                    span[i] = _data[i] + temp;           
                throw new NotImplementedException();
#endif
                return new Image<T>(span, Height, Width);
            }
        }

        public IImmutableImage<T> MultiplyBy(T item)
        {
#if !ALLOW_UNSAFE_IL_MATH
            dynamic temp = item;
#endif

            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);

                for (var i = 0; i < _data.Length; i++)
#if ALLOW_UNSAFE_IL_MATH
                    span[i] = DangerousMultiply(_data[i], item);
#else
                    span[i] = _data[i] * temp;           
                throw new NotImplementedException();
#endif
                return new Image<T>(span, Height, Width);
            }
        }

        public IImmutableImage<T> DivideBy(T item)
        {
#if !ALLOW_UNSAFE_IL_MATH
            dynamic temp = item;
#endif

            using (var mem = MemoryPool<T>.Shared.Rent(Width * Height))
            {
                var span = mem.Memory.Span.Slice(0, Width * Height);

                for (var i = 0; i < _data.Length; i++)
#if ALLOW_UNSAFE_IL_MATH
                    span[i] = DangerousDivide(_data[i], item);
#else
                    span[i] = _data[i] / temp;           
                throw new NotImplementedException();
#endif
                return new Image<T>(span, Height, Width);
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double IImmutableImage.Min()
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Min());
#else
            return (double) Convert.ChangeType(Min(), typeof(double));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double IImmutableImage.Max()
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Max());
#else
            return (double) Convert.ChangeType(Max(), typeof(double));
#endif
        }

        IImmutableImage IImmutableImage.Clamp(double low, double high)
        {
#if ALLOW_UNSAFE_IL_MATH
            return Clamp(DangerousCast<double, T>(low), DangerousCast<double, T>(high));
#else
            return Clamp((T) Convert.ChangeType(low, typeof(T)), (T) Convert.ChangeType(high, typeof(T)));
#endif
        }

        double IImmutableImage.Percentile(double lvl)
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Percentile(DangerousCast<double, T>(lvl)));
#else
            return (double) Convert.ChangeType(Percentile((T) Convert.ChangeType(lvl, typeof(T))), typeof(double));
#endif
        }

        public override bool Equals(object obj)
            => obj is IImmutableImage<T> other
               && Equals(other);

        // TODO : Fix poor hash function
        public override int GetHashCode()
            => _data.GetHashCode() ^ ((Width << 16 ) ^ Height);

        private static void ThrowIfTypeMismatch()
        {
            if (!AllowedTypes.Contains(typeof(T)))
                throw new NotSupportedException(typeof(T).ToString());
        }


    }
}