using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Internal.UnsafeNumerics;

namespace CopyBenchmarks
{

    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    public class Intrinsics<T> where T : unmanaged, IComparable<T>
    {
        private T[] _array_1;
        private T[] _array_2;

        private Random _r;
        [Params(1_000, 100_000, 1_000 * 1_000)]
        public int N;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _r = new Random();
        }

        [IterationSetup]
        public void IterSetup()
        {
            _array_1 = new T[N];
            _array_2 = new T[N];
            for (var i = 0; i < N; i++)
            {
                _array_1[i] = (T)Convert.ChangeType(_r.NextDouble(), typeof(T));
                _array_2[i] = (T)Convert.ChangeType(_r.NextDouble(), typeof(T));
            }
        }

        [Benchmark(Baseline = true)]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void IComparable()
        {
            var coll = _array_1.Cast<IComparable<T>>();

            var sum = 0;

            foreach (var item in coll)
                sum += item.CompareTo(default);
        }

        [Benchmark()]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void IL()
        {
            var coll = _array_2.AsEnumerable();

            var sum = 0;

            foreach (var item in coll)
                sum += MathOps.DangerousCompare(item, default);
        }

        [Benchmark()]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void DynamicAdd()
        {
            dynamic sum = 0;

            foreach (var item in _array_1)
                sum += item;
        }

        [Benchmark()]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ILAdd()
        {
            T sum = default;

            foreach (var item in _array_1)
                sum = MathOps.DangerousAdd(sum, item);
        }
    }
}
