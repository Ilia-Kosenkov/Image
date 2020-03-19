using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if ALLOW_UNSAFE_IL_MATH
using static Internal.UnsafeNumerics.MathOps;
#else
using static Internal.Numerics.MathOps;
#endif

namespace ImageCore
{
    [DebuggerDisplay("{" + nameof(Size) + "}")]
    public sealed class SubImage<T> : ISubImage<T>
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        private readonly IImage<T> _sourceImage;
        private readonly ImmutableArray<(int I, int J)> _indexes;

        private T? _max;
        private T? _min;
        private double? _average;
        private double? _var;
        private T? _median;

        public long Size => _indexes.Length;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                var (i, j) = _indexes[index];

                return _sourceImage[i, j];
            }
        }

        public T this[Index i] => this[i.GetOffset(_indexes.Length)];

        internal SubImage(IImage<T> source, IReadOnlyCollection<(int I, int J)> indexCollection)
        {
            _sourceImage = source ?? throw new ArgumentNullException(nameof(source));
            if (indexCollection is null)
                throw new ArgumentNullException(nameof(indexCollection));
            if (indexCollection.Count == 0)
                throw new ArgumentException(nameof(indexCollection));

            _indexes = indexCollection.ToImmutableArray();
            foreach (ref readonly var item in _indexes.AsSpan())
            {
                if (item.I < 0 || item.I > source.Height)
                    throw new ArgumentOutOfRangeException(nameof(indexCollection), item, nameof(item.I));
                if (item.J < 0 || item.J > source.Width)
                    throw new ArgumentOutOfRangeException(nameof(indexCollection), item, nameof(item.J));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Max()
        {
            if (_max is null)
            {
                var (i0, j0) = _indexes[0];
                var maxVal = _sourceImage[i0, j0];

                for (var index = 1; index < Size; index++)
                {
                    var (i, j) = _indexes[index];
                    var val = _sourceImage[i, j];
                    if (DangerousGreaterThan(val, maxVal))
                        maxVal = val;
                }

                _max = maxVal;
            }

            return _max.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Min()
        {
            if (_min is null)
            {
                var (i0, j0) = _indexes[0];
                var minVal = _sourceImage[i0, j0];

                for (var index = 1; index < Size; index++)
                {
                    var (i, j) = _indexes[index];
                    var val = _sourceImage[i, j];
                    if (DangerousLessThan(val, minVal))
                        minVal = val;
                }

                _min = minVal;
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
            var len = (int)Math.Ceiling(
                DangerousCast<T, double>(
                    DangerousMultiply(
                        lvl,
                        DangerousCast<long, T>(Size))) / 100.0);


            if (len < 1)
                len = 1;

            var idxLen = _indexes.Length;
            var buff = ArrayPool<T>.Shared.Rent(idxLen);
            try
            {
                for (var i = 0; i < idxLen; i++)
                    buff[i] = _sourceImage[_indexes[i]];

                Array.Sort(buff, 0, idxLen);

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
                _median = Percentile(DangerousCast<int, T>(50));

            return _median.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Average()
        {
            if (_average is null)
            {
                var sum = 0.0;
                foreach (var (i, j) in _indexes)
                    sum += DangerousCast<T, double>(_sourceImage[i, j]);
                
                _average = sum / Size;
            }
            return DangerousCast<double, T>(_average.Value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Var()
        {
            // ReSharper disable once PossibleInvalidOperationException
            if (_var is {}) return DangerousCast<double, T>(_var.Value);
            if (Size > 1)
            {
                var avg = Average();
                var sum = 0.0;
                foreach (var (i, j) in _indexes)
                {
                    var val = _sourceImage[i, j];

                    var diff = DangerousCast<T, double>(DangerousSubtract(val, avg));
                    sum += diff * diff;
                }

                _var = sum / (Size - 1);
            }
            else
                _var = 0.0;

            return DangerousCast<double, T>(_var.Value);
        }
      
    }
}
