using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
#if ALLOW_UNSAFE_IL_MATH
using static Internal.UnsafeNumerics.MathOps;
#else
using static Internal.Numerics.MathOps;
#endif

namespace ImageCore
{
    public abstract class Image
    {
        public delegate void Initializer<T>(Span<T> span)
            where T : unmanaged, IComparable<T>, IEquatable<T>;

        public static IImmutableList<Type> AllowedTypes { get; } =
            new[] 
            {
                typeof(double),
                typeof(float),
                typeof(ulong),
                typeof(uint),
                typeof(ushort),
                typeof(byte),
                typeof(long),
                typeof(int),
                typeof(short),
                typeof(sbyte),
            }.ToImmutableList();
        private protected static void ThrowIfTypeMismatch<T>()
        {
            if (!AllowedTypes.Contains(typeof(T)))
                throw new NotSupportedException(typeof(T).ToString());
        }

        public static IImage<T> Create<T>(Initializer<T> init, int height, int width)
            where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            if (init is null)
                throw new ArgumentNullException(nameof(init));
            var img = new Image<T>(height, width);
            init(img.RawView);
            return img;
        }

    }

    [Serializable]
    public sealed class Image<T> : Image, IImage<T> 
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
 
        private readonly T[] _data;
        private T? _max;
        private T? _min;
        private double? _average;
        private double? _var;
        private T? _median;

        internal Span<T> RawView => _data;

        public long Size => Height * Width;

        public int Height { get; }
        public int Width { get; }

        public T this[int i, int j] =>
            i < 0 || i >= Height
                ? throw new ArgumentOutOfRangeException(nameof(i))
                : j < 0 || j >= Width
                    ? throw new ArgumentOutOfRangeException(nameof(j))
                    : _data[i * Width + j];

        public T this[long i] => i < 0 || i >= Size
            ? throw new ArgumentOutOfRangeException(nameof(i))
            : _data[i];

        internal Image(int height, int width)
        {
            ThrowIfTypeMismatch<T>();

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            _data = new T[width * height];
            Width = width;
            Height = height;
        }

        public Image(ReadOnlySpan<T> data, int height, int width)
        {
           ThrowIfTypeMismatch<T>();

            if(width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
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
            ThrowIfTypeMismatch<T>();

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var size = Unsafe.SizeOf<T>();

            // Size mismatch
            if (size * byteData.Length < width * height)
                throw new ArgumentException();

            _data = new T[width * height];


            MemoryMarshal.Cast<byte, T>(byteData).CopyTo(_data);

            Width = width;
            Height = height;
        }

        private Image(SerializationInfo info, StreamingContext context)
        {
            ThrowIfTypeMismatch<T>();


            // WATCH : Weak type checks
            var type = info.GetString("Type");
            if(type != typeof(T).FullName)
                throw new ArrayTypeMismatchException();

            var width = info.GetInt32("Width");
            var height = info.GetInt32("Height");
            var byteData = info.GetValue("ByteData", typeof(byte[])) as byte[] 
                       ?? throw new InvalidOperationException();


            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var size = Unsafe.SizeOf<T>();

            // Size mismatch
            if (size * byteData.Length < width * height)
                throw new ArgumentException();

            _data = new T[width * height];


            MemoryMarshal.Cast<byte, T>(byteData).CopyTo(_data);

            Width = width;
            Height = height;
        }

        public ref readonly T DangerousGet(long pos)
            => ref Unsafe.Add(ref _data.AsSpan().GetPinnableReference(), new IntPtr(pos));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Max()
        {
            if (_max is null)
            {
                var temp = _data[0];
                foreach (var item in _data)
                    if (DangerousGreaterEquals(item, temp))
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
                    if (DangerousLessEquals(item, temp))

                        temp = item;

                _min = temp;
            }

            return _min.Value;
        }

        public T Percentile(T lvl)
        {
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
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Median()
        {
            if (_median is null)
            {
                _median = Percentile(DangerousCast<int, T>(50));
            }

            return _median.Value;
        }

        public T Average()
        {
            return DangerousCast<double, T>((this as ISubImage).Average());
        }

        public T Var()
        {
            return DangerousCast<double, T>((this as ISubImage).Var());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetByteView()
            => MemoryMarshal.Cast<T, byte>(new ReadOnlySpan<T>(_data));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetView() => _data;

        public IImage<T> Copy()
            => new Image<T>(_data, Height, Width);

        public IImage<T> Transpose()
        {
            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);
                
            for(var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                span[j * Height + i] = _data[i * Width + j];

            return  new Image<T>(span, Width, Height);
        }

        public IImage<TOther> CastTo<TOther>() where TOther 
            : unmanaged, IComparable<TOther>, IEquatable<TOther>
        {
            using var pool = MemoryPool<TOther>.Shared.Rent(Width * Height);
            var span = pool.Memory.Span.Slice(0, Width * Height);
            for (var i = 0; i < _data.Length; i++)
                span[i] = DangerousCast<T, TOther>(_data[i]);
            return new Image<TOther>(span, Height, Width);
        }

        public IImage<TOther> CastTo<TOther>(Func<T, TOther> caster) 
            where TOther : unmanaged, IComparable<TOther>, IEquatable<TOther>
        {
            using var pool = MemoryPool<TOther>.Shared.Rent(Width * Height);
            var span = pool.Memory.Span.Slice(0, Width * Height);
            for (var i = 0; i < _data.Length; i++)
                span[i] = caster(_data[i]);

            return new Image<TOther>(span, Height, Width);
        }

        public IImage<T> Clamp(T low, T high)
        {
            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);
            _data.AsSpan().CopyTo(span);

            foreach (ref var item in span)
                if (DangerousLessThan(item, low))
                    item = low;
                else if(DangerousGreaterThan(item ,high))
                    item = high;
            return new Image<T>(span, Height, Width);
        }

        public IImage<T> Scale(T low, T high)
        {
            var min = Min();
            var max = Max();
            var enumer = DangerousSubtract(high, low);
            var denomer = DangerousSubtract(max, min);

            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
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

        public IImage<T> AddScalar(T item)
        {
            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);

            for (var i = 0; i < _data.Length; i++)
                span[i] = DangerousAdd(_data[i], item);
            return new Image<T>(span, Height, Width);
        }

        public IImage<T> MultiplyBy(T item)
        {
            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);

            for (var i = 0; i < _data.Length; i++)
                span[i] = DangerousMultiply(_data[i], item);
            return new Image<T>(span, Height, Width);
        }

        public IImage<T> DivideBy(T item)
        {
            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);

            for (var i = 0; i < _data.Length; i++)
                span[i] = DangerousDivide(_data[i], item);
            return new Image<T>(span, Height, Width);
        }

        public IImage<T> Add(IImage<T> other)
        {
            if (Width != other.Width && Height != other.Height)
                throw new ArgumentException(nameof(other));

            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);
            var view = other.GetView();
            for (var i = 0; i < _data.Length; i++)
                span[i] = DangerousAdd(_data[i], view[i]);
            return new Image<T>(span, Height, Width);
        }

        public IImage<T> Subtract(IImage<T> other)
        {
            if (Width != other.Width && Height != other.Height)
                throw new ArgumentException(nameof(other));

            using var mem = MemoryPool<T>.Shared.Rent(Width * Height);
            var span = mem.Memory.Span.Slice(0, Width * Height);
            var view = other.GetView();
            for (var i = 0; i < _data.Length; i++)
                span[i] = DangerousSubtract(_data[i], view[i]);
            return new Image<T>(span, Height, Width);
        }

        public ISubImage<T> Slice(ICollection<(int I, int J)> indexes) 
            => new SubImage<T>(this, indexes);

        public ISubImage<T> Slice(Func<T, bool> selector)
        {
            var indexes = new List<(int I, int J)>();
            for(var i = 0; i < Height; i++)
                for(var j = 0; j < Width; j++)
                    if(selector(this[i, j]))
                        indexes.Add((i, j));

            if (indexes.Count == Size)
                return this;

            return new SubImage<T>(this, indexes);
        }

        public ISubImage<T> Slice(Func<int, int, T, bool> selector)
        {
            var indexes = new List<(int I, int J)>();
            for (var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                    if (selector(i, j, this[i, j]))
                        indexes.Add((i, j));

            if (indexes.Count == Size)
                return this;

            return new SubImage<T>(this, indexes);
        }

        public bool Equals(IImage<T> other)
        {
            if (other is null || Width != other.Width || Height != other.Height)
                return false;

            // WATCH: Possible optimization
            var view = other.GetView();
            //for(var i = 0; i < Width * Height; i++)
            //    if (DangerousNotEquals(_data[i], view[i]))
            //        return false;
            //return true;
            return GetView().SequenceEqual(view);
        }

        public bool Equals(IImage other)
            => other is IImage<T> img
               && Equals(img);
        public bool BitwiseEquals(IImage other)
        {
            if (!(other is IImage<T> img) || Width != img.Width || Height != img.Height)
                return false;
            return GetByteView().SequenceEqual(img.GetByteView());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Clone() => Copy();


#region IImage

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IImage IImage.Add(IImage other)
            => other is IImage<T> img
                ? Add(img)
                : throw new ArgumentException(nameof(other));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IImage IImage.Subtract(IImage other)
            => other is IImage<T> img
                ? Subtract(img)
                : throw new ArgumentException(nameof(other));

        ISubImage IImage.Slice(ICollection<(int I, int J)> pixels) 
            => Slice(pixels);

        ISubImage IImage.Slice(Func<double, bool> selector)
        {
            bool Func(T x) => selector(DangerousCast<T, double>(x));
            return Slice(Func);
        }

        ISubImage IImage.Slice(Func<int, int, double, bool> selector)
        {
            bool Func(int i, int j, T x) => selector(i, j, DangerousCast<T, double>(x));
            return Slice(Func);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double ISubImage.Min()
        {
            return DangerousCast<T, double>(Min());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double ISubImage.Max()
        {
            return DangerousCast<T, double>(Max());
        }

        IImage IImage.Clamp(double low, double high)
        {
            return Clamp(DangerousCast<double, T>(low), DangerousCast<double, T>(high));
        }

        double ISubImage.Percentile(double lvl)
        {
            return DangerousCast<T, double>(Percentile(DangerousCast<double, T>(lvl)));
        }

        double ISubImage.Median()
        {
            return DangerousCast<T, double>(Median());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        double ISubImage.Var()
        {
            if (!(_var is null)) return _var.Value;

            if (Size > 1)
            {
                var avg = Average();
                var sum = 0.0;
                foreach(var item in _data)
                { 
                    var diff = DangerousCast<T, double>(DangerousSubtract(item, avg));

                    sum += diff * diff;
                }

                _var = sum / (Size - 1);
            }
            else
                _var = 0.0;

            return _var.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        double ISubImage.Average()
        {
            if(_average is null)
            {
                var sum = 0.0;
                foreach (var item in _data)
                    sum += DangerousCast<T, double>(item);

                _average = sum / Size;
            }
            return _average.Value;
        }

#endregion

        public IEnumerator<T> GetEnumerator()
            => (_data as IEnumerable<T>).GetEnumerator();

        public override bool Equals(object obj)
            => obj is IImage<T> other
               && Equals(other);

        public override int GetHashCode()
            => (int) unchecked((Internals.CRC32Generator.ComputeHash<T>(_data) * 31 + (uint) Width) * 31 + (uint) Height);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Width", Width);
            info.AddValue("Height", Height);
            info.AddValue("Type", typeof(T).FullName);
            info.AddValue("ByteData", GetByteView().ToArray());
        }

        public static IImage<T> Zero(int height, int width)
            => new Image<T>(height, width);

    }
}