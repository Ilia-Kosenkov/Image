#nullable enable

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using ImageCore.Internals;
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

        public static ImmutableArray<Type> AllowedTypes { get; } =
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
                typeof(sbyte)
            }.ToImmutableArray();

        private protected static void ThrowIfTypeMismatch<T>() where T : unmanaged
        {
            if (!IsTypeAllowed<T>())
                throw new NotSupportedException(typeof(T).ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTypeAllowed<T>() where T : unmanaged 
            => default(T) switch
            {
                double {} => true,
                float {} => true,
                ulong {} => true,
                uint {} => true,
                ushort {} => true,
                byte {} => true,
                long {} => true,
                int {} => true,
                short {} => true,
                sbyte {} => true,
                _ => false
            };

        public static bool IsTypeAllowed(Type type)
        {
            if (type is null)
                return false;

            if(type == typeof(double))
				return true;
            if(type == typeof(float))
				return true;
            if(type == typeof(ulong))
				return true;
            if(type == typeof(uint))
				return true;
            if(type == typeof(ushort))
				return true;
            if(type == typeof(byte))
				return true;
            if(type == typeof(long))
				return true;
            if(type == typeof(int))
				return true;
            if(type == typeof(short))
				return true;
            if(type == typeof(sbyte))
				return true;

            return false;
        }

        public static IImage<T> Create<T>(Initializer<T> init, int height, int width)
            where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            return new Image<T>(height, width, init);
        }

        public static IImage<T> CreateRaw<T>(Initializer<byte> init, int height, int width)
            where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            return new Image<T>(height, width, init);
        }

        public static IImage<T> Create<T>(ReadOnlySpan<T> data, int height, int width)
            where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            return new Image<T>(data, height, width);
        }

        public static IImage<T> Create<T>(ReadOnlySpan<byte> data, int height, int width)
            where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            return new Image<T>(data, height, width);
        }

        public static IImage<T> Create<T>(T[,] data)
            where T : unmanaged, IComparable<T>, IEquatable<T>
        {
            _ = data ?? throw new ArgumentNullException(nameof(data));
            var (height, width) = (data.GetLength(0), data.GetLength(1));
            var view = MemoryMarshal.CreateReadOnlySpan(ref data[0, 0], data.Length);

            return new Image<T>(view, height, width);
        }

    }

    [Serializable]
    [DebuggerDisplay("W:{" + nameof(Width) + "} x H:{" + nameof(Height) + "}")]
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

        public T this[Index i, Index j] =>
            this[i.GetOffset(Width), j.GetOffset(Height)];
        public T this[(int I, int J) index] => this[index.I, index.J];

        public T this[int i] => i < 0 || i >= Size
            ? throw new ArgumentOutOfRangeException(nameof(i))
            : _data[i];

        public T this[Index i] => this[i.GetOffset(Width * Height)];

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

        internal Image(int height, int width, Initializer<T> filler)
        {
            ThrowIfTypeMismatch<T>();

            if (filler is null)
                throw new ArgumentNullException(nameof(filler));
            
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

          
            _data = new T[width * height];
            Width = width;
            Height = height;

            filler(_data);
        }

        internal Image(int height, int width, Initializer<byte> filler)
        {
            ThrowIfTypeMismatch<T>();

            if (filler is null)
                throw new ArgumentNullException(nameof(filler));

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));


            _data = new T[width * height];
            Width = width;
            Height = height;
            filler(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref _data[0]), _data.Length * Unsafe.SizeOf<T>()));
        }

        internal Image(ReadOnlySpan<T> data, int height, int width)
        {
           ThrowIfTypeMismatch<T>();

            if(width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            // Size mismatch
            if (data.Length > width * height)
                throw new ArgumentException(nameof(data));


            _data = new T[width * height];
            data.CopyTo(_data);
            Width = width;
            Height = height;
        }

        internal Image(ReadOnlySpan<byte> byteData, int height, int width)
        {
            ThrowIfTypeMismatch<T>();

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var size = Unsafe.SizeOf<T>();

            // Size mismatch
            if (byteData.Length > width * height * size * size)
                throw new ArgumentException(nameof(byteData));

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
            => ref Unsafe.Add(ref MemoryMarshal.GetReference<T>(_data), new IntPtr(pos));

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

                return _max.Value;
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

            if(len > Width * Height)
                throw new InvalidOperationException("Should not happen.");

            if (len < 1)
                len = 1;

            var buff = ArrayPool<T>.Shared.Rent(Width * Height);
            try
            {
                _data.CopyTo(buff.AsSpan(0, Width * Height));
                Array.Sort(buff, 0, Width * Height);

                return buff[len - 1];
            }
            finally
            {
                ArrayPool<T>.Shared.Return(buff);
            }
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Var()
        {
            if (_var is null)
            {
                if (Size > 1)
                {
                    var len = _data.Length;
                    var avg = Average();
                    ref readonly var item = ref MemoryMarshal.GetReference((ReadOnlySpan<T>) _data);
                    var sum = DangerousCast<T, double>(DangerousSubtract(item, avg));
                    sum *= sum;

                    for (var i = 1; i < len; i++)
                    {
                        item = ref Unsafe.Add(ref Unsafe.AsRef(item), 1);
                        var diff = DangerousCast<T, double>(DangerousSubtract(item, avg));
                        sum += diff * diff;
                    }
                    _var = sum / (Size - 1);
                }
                else
                    _var = 0.0;
            }
            return DangerousCast<double, T>(_var.Value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Average()
        {
            if (_average is null)
            {
                var len = _data.Length;
                ref readonly var item = ref MemoryMarshal.GetReference((ReadOnlySpan<T>)_data);

                var sum = DangerousCast<T, double>(item);

                for (var i = 1; i < len; i++)
                {
                    item = ref Unsafe.Add(ref Unsafe.AsRef(item), 1);
                    sum += DangerousCast<T, double>(item);
                }
                
                _average = sum / Size;
            }
            return DangerousCast<double, T>(_average.Value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetByteView()
            => MemoryMarshal.Cast<T, byte>(new ReadOnlySpan<T>(_data));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetView() => _data;

        public IImage<T> Copy()
            => new Image<T>(_data, Height, Width);

        public IImage<T> Transpose() 
            => new Image<T>(Width, Height, span =>
            {
                ImageTransImpl.Transform(_data, span, Height, Width, ImageTransImpl.GetTransposeSelector());
            });

        public IImage<T> Rotate(RotationDegree degree)
        {
            var (width, height) = degree == RotationDegree.Rotate90 || degree == RotationDegree.Rotate270
                ? (Height, Width)
                : (Width, Height);

            return new Image<T>(height, width,
                span => ImageTransImpl.Transform(_data, span, Height, Width,
                    ImageTransImpl.GetRotationSelector(degree)));
        }

        public IImage<T> Flip(FlipDirection direction)
            => new Image<T>(Height, Width,
                span => ImageTransImpl.Transform(_data, span, Height, Width,
                    ImageTransImpl.GetFlipSelector(direction)));

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

        public ISubImage<T> Slice(IImmutableList<(int I, int J)> indexes) 
            => new SubImage<T>(this, indexes);

        public ISubImage<T> Slice(Func<T, bool> selector)
        {
            var indexes = new List<(int I, int J)>(Width * Height / 32);
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
            var indexes = new List<(int I, int J)>(Width * Height / 32);
            for (var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                    if (selector(i, j, this[i, j]))
                        indexes.Add((i, j));

            if (indexes.Count == Size)
                return this;

            return new SubImage<T>(this, indexes);
        }
        public ISubImage<T> Slice(Range horizontal, Range vertical)
        {
            var x = horizontal.GetOffsetAndLength(Height);
            var y = horizontal.GetOffsetAndLength(Width);

            var result = new List<(int I, int J)>(x.Length * y.Length);
            for (var i = x.Offset; i < x.Length; i++)
            for (var j = y.Offset; j < y.Length; j++)
                result.Add((i, j));

            return Slice(result.ToImmutableList());
        }

        public bool Equals(IImage<T> other)
        {
            if (other is null || Width != other.Width || Height != other.Height)
                return false;

            var that = other.GetView();
            var @this = GetView();
            
            if (typeof(T) != typeof(float) && typeof(T) != typeof(double)) 
                return @this.SequenceEqual(that);

            for (var i = 0; i < Width * Height; i++)
                if (DangerousNotEquals(@this[i], that[i]))
                    return false;
            return true;
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


        public IEnumerator<T> GetEnumerator()
            => (_data as IEnumerable<T>).GetEnumerator();

        public override bool Equals(object obj)
            => obj is IImage<T> other
               && Equals(other);

        public override int GetHashCode()
            => (int) unchecked((CRC32Generator.ComputeHash<T>(_data) * 31 + (uint) Width) * 31 + (uint) Height);

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