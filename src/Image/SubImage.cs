using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using static Internal.Numerics.MathOps;

namespace Image
{
    public sealed class SubImage<T> : ISubImage<T>
        where T : unmanaged, IComparable<T>
    {
        private readonly IImage<T> _sourceImage;
        private readonly List<(int I, int J)> _indexes;

        private T? _max;
        private T? _min;

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
            throw new NotImplementedException();
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

        public double Percentile(double lvl)
        {
            throw new NotImplementedException();
        }

    }
}
