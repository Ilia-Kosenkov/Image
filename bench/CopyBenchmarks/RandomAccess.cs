using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace CopyBenchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    public class RandomAccess
    {
        private Random R;
        private float[] dArray;
        private byte[] bArray;

        private int[] _indices;


        [Params(1025, 1024 * 1025, 67 * 1024 * 1024)]
        public int N;

        //[Params(1, 2, 4)]
        //public int Step;

        [GlobalSetup]
        public void GlobalSetup()
        {
            R = new Random();
            dArray = new float[N];
            bArray = new byte[N * sizeof(float)];
            R.NextBytes(bArray);
            Buffer.BlockCopy(bArray, 0, dArray, 0, bArray.Length);

            _indices = new int[N / 16];

        }

        [IterationSetup]
        public void IterSetup()
        {
            for (var i = 0; i < _indices.Length; i++)
                _indices[i] = R.Next() % N;
        }

        [Benchmark(Baseline = true)]
        public void Float_Sum_Float()
        {
            Span<float> span = dArray;
            var sum = 0.0;
            for (var i = 0; i < _indices.Length; i++)
                sum += span[_indices[i]];
        }

        [Benchmark]

        public void Float_Sum_Byte()
        {
            var span = bArray.AsSpan().TypeCast<byte, float>();
            var sum = 0.0;
            for (var i = 0; i < _indices.Length; i++)
                sum += span[_indices[i]];
        }

        [Benchmark]

        public void Byte_Sum_Float_2()
        {
            var span = dArray.AsSpan().TypeCast2<float, byte>();
            var sum = 0.0;
            for (var i = 0; i < _indices.Length; i++)
                sum += span[_indices[i]];
        }

        [Benchmark]

        public void Float_Sum_Byte_2()
        {
            var span = bArray.AsSpan().TypeCast2<byte, float>();
            var sum = 0.0;
            for (var i = 0; i < _indices.Length; i++)
                sum += span[_indices[i]];
        }

        [Benchmark]

        public void Byte_Sum_Float()
        {
            var span = dArray.AsSpan().TypeCast<float, byte>();
            var sum = 0.0;
            for (var i = 0; i < _indices.Length; i++)
                sum += span[_indices[i]];
        }


        [Benchmark]

        public void Byte_Sum_Byte()
        {
            var span = bArray.AsSpan();
            var sum = 0.0;
            for (var i = 0; i < _indices.Length; i++)
                sum += span[_indices[i]];
        }
    }
}
