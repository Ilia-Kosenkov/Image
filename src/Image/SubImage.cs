using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Internal.Numerics.MathOps;

namespace ImageCore
{
    public sealed class SubImage<T> : ISubImage<T>
        where T : unmanaged, IComparable<T>
    {
        private readonly IImage<T> _sourceImage;
        private readonly List<(int I, int J)> _indexes;

        private T? _max;
        private T? _min;
        private T? _average;
        private T? _var;
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
                var ind = _indexes[0];
                var maxVal = _sourceImage[ind.I, ind.J];

                for (var index = 1; index < Size; index++)
                {
                    var (i, j) = _indexes[index];
                    var val = _sourceImage[i, j];
#if ALLOW_UNSAFE_IL_MATH
                    if (DangerousGreaterThan(val, maxVal))
#else
                    if(val.CompareTo(maxVal) > 0)
#endif
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
                var ind = _indexes[0];
                var minVal = _sourceImage[ind.I, ind.J];

                for (var index = 1; index < Size; index++)
                {
                    var (i, j) = _indexes[index];
                    var val = _sourceImage[i, j];
#if ALLOW_UNSAFE_IL_MATH
                    if (DangerousLessThan(val, minVal))
#else
                    if(val.CompareTo(minVal) < 0)
#endif
                        minVal = val;
                }

                _min = minVal;
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
#else
            dynamic l = lvl;
            if (Math.Abs(l) < double.Epsilon)
                return Min();
            if (Math.Abs(l - 1) < double.Epsilon)
                return Max();
            var query = _indexes.Select(x => _sourceImage[x.I, x.J]).OrderBy(x => x, Comparer<T>.Default);

            var len = (int)Math.Ceiling(l * Size / 100.0);

            if (len < 1)
                len = 1;

            return query.Skip(len - 1).Take(1).First();
#endif
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Median()
        {
            if (_median is null)
            {
#if ALLOW_UNSAFE_IL_MATH
                _median = Percentile(DangerousCast<int, T>(50));
#else
            _median = Percentile((T)Convert.ChangeType(50, typeof(T)));
#endif
            }

            return _median.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Average()
        {
            if (_average is null)
            {
#if ALLOW_UNSAFE_IL_MATH
                T temp = default;
                foreach (var item in _indexes.Select(x => _sourceImage[x.I, x.J]))
                    temp = DangerousAdd(temp, item);

                _average = DangerousDivide(temp, DangerousCast<long, T>(Size));
#else
                dynamic temp = default(T);
                foreach (var item in _indexes.Select(x => _sourceImage[x.I, x.J]))
                    temp += item;

                _average = temp / Size;
#endif
            }

            return _average.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T Var()
        {
            if (_var is null)
            {
#if ALLOW_UNSAFE_IL_MATH
                T temp = default;
                var avg = Average();

                foreach (var item in _indexes.Select(x => _sourceImage[x.I, x.J]))
                {
                    var diff = DangerousSubtract(item, avg);
                    temp = DangerousAdd(temp, DangerousMultiply(diff, diff));
                }

                _var = DangerousDivide(temp, DangerousCast<long, T>(Size - 1));
#else
                dynamic temp = default(T);
                dynamic avg = Average();
                foreach (var item in _indexes.Select(x => _sourceImage[x.I, x.J]))
                    temp += (item - avg) * (item - avg);

                _var = temp / (Size - 1);
#endif
            }

            return _var.Value;
        }

        double ISubImage.Min()
#if ALLOW_UNSAFE_IL_MATH
            => DangerousCast<T, double>(Min());
#else
            => (double) Convert.ChangeType(Min(), typeof(double));
#endif
        double ISubImage.Max()
#if ALLOW_UNSAFE_IL_MATH
            => DangerousCast<T, double>(Max());
#else
            => (double) Convert.ChangeType(Max(), typeof(double));
#endif


        double ISubImage.Median()
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Median());
#else
            return (double)Convert.ChangeType(Median(), typeof(double));
#endif
        }

        double ISubImage.Var()
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Var());
#else
            return (double)Convert.ChangeType(Var(), typeof(double));
#endif
        }

        double ISubImage.Average()
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Average());
#else
            return (double)Convert.ChangeType(Average(), typeof(double));
#endif
        }


        public double Percentile(double lvl)
        {
#if ALLOW_UNSAFE_IL_MATH
            return DangerousCast<T, double>(Percentile(DangerousCast<double, T>(lvl)));
#else
            return (double) Convert.ChangeType(Percentile((T) Convert.ChangeType(lvl, typeof(T))), typeof(double));
#endif
        }


    }
}
