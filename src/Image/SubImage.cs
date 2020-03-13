using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
#if ALLOW_UNSAFE_IL_MATH
using static Internal.UnsafeNumerics.MathOps;
#else
using static Internal.Numerics.MathOps;
#endif

namespace ImageCore
{
    [DebuggerDisplay("{Size}")]
    public sealed class SubImage<T> : ISubImage<T>
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        private readonly IImage<T> _sourceImage;
        private readonly List<(int I, int J)> _indexes;

        private T? _max;
        private T? _min;
        private double? _average;
        private double? _var;
        private T? _median;

        public long Size => _indexes.Count;

        public T this[long index]
        {
            get
            {
                if (index < 0 || index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                var (i, j) = _indexes[(int)index];

                return _sourceImage[i, j];
            }
        }

        internal SubImage(IImage<T> source, ICollection<(int I, int J)> indexCollection)
        {
            _sourceImage = source ?? throw new ArgumentNullException(nameof(source));
            if (indexCollection is null)
                throw new ArgumentNullException(nameof(indexCollection));
            if (indexCollection.Count == 0)
                throw new ArgumentException(nameof(indexCollection));

            _indexes = indexCollection.ToList();
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

            return _indexes.Select(x => _sourceImage[x.I, x.J])
                .OrderBy(x => x, Comparer<T>.Default)
                .Skip(len - 1).First();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Median()
        {
            if (_median is null)
                _median = Percentile(DangerousCast<int, T>(50));

            return _median.Value;
        }
        public T Average() 
            => DangerousCast<double, T>((this as ISubImage).Average());

        public T Var() 
            => DangerousCast<double, T>((this as ISubImage).Var());

        double ISubImage.Min()
            => DangerousCast<T, double>(Min());
        double ISubImage.Max()
            => DangerousCast<T, double>(Max());

        double ISubImage.Median() 
            => DangerousCast<T, double>(Median());

        [MethodImpl(MethodImplOptions.Synchronized)]
        double ISubImage.Var()
        {
            if (_var is null)
            {
                if (Size > 1)
                {
                    var avg = Average();
                    var sum = 0.0;
                    foreach (var item in _indexes.Select(x => _sourceImage[x.I, x.J]))
                    {
                        var diff = DangerousCast<T, double>(DangerousSubtract(item, avg));
                        sum += diff * diff;
                    }

                    _var = sum / (Size - 1);
                }
                else
                    _var = 0.0;
            }

            return _var.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        double ISubImage.Average()
        {
            if (_average is null)
            {
                var sum = 0.0;
                foreach (var item in _indexes.Select(x => _sourceImage[x.I, x.J]))
                    sum += DangerousCast<T, double>(item);
                _average = sum / Size;
            }
            return _average.Value;
        }


        public double Percentile(double lvl) 
            => DangerousCast<T, double>(Percentile(DangerousCast<double, T>(lvl)));
    }
}
