using System;
using BenchmarkDotNet.Attributes;

namespace CopyBenchmarks
{
    [CoreJob()]
    public class AccessBench
    {
        public Random R;
        public int[] Arr;

        [Params(1024, 512*1024, 2 * 1024 * 1024, 8 * 1024 * 1024, 32 * 1024 * 1024)]
        public int N;

        [GlobalSetup]
        public void GlobalSetup()
        {
            R = new Random();
        }

        [IterationSetup]
        public void IterSetup()
        {
            Arr = new int[N];

            for (var i = 0; i < N; i++)
                Arr[i] = R.Next();


        }

        [Benchmark(Baseline = true)]
        public void BitwiseFlip()
        {
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            static int Flip(int input)
                => (int) (((input & 0x_00_00_00_FF) << 24)
                          | ((input & 0x_00_00_FF_00) << 8)
                          | ((input & 0x_00_FF_00_00) >> 8)
                          | ((input & 0x_FF_00_00_00) >> 24));
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

            for (var i = 0; i < N; i++)
                Arr[i] = Flip(Arr[i]);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public unsafe void PtrFlip()
        {
            static void Flip(byte* ptr)
            {
                var buff = ptr[0];
                ptr[0] = ptr[3];
                ptr[3] = buff;
                buff = ptr[1];
                ptr[1] = ptr[2];
                ptr[2] = buff;
            }

            fixed (int* sptr = Arr)
            {
                var p = (byte*) sptr;
                for (var i = 0; i < N; i++)
                {
                   Flip(p + i * sizeof(int));
                }
            }
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public void SpanFlip()
        {
            static void Flip(Span<byte> ptr)
            {
                var buff = ptr[0];
                ptr[0] = ptr[3];
                ptr[3] = buff;
                buff = ptr[1];
                ptr[1] = ptr[2];
                ptr[2] = buff;
            }

            var arr = Arr.AsSpan().TypeCast<int, byte>();

            for (var i = 0; i < N; i++)
                Flip(arr.Slice(i * sizeof(int), sizeof(int)));

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public void SpanFlip2()
        {
            static void Flip(Span<byte> ptr)
            {
                var buff = ptr[0];
                ptr[0] = ptr[3];
                ptr[3] = buff;
                buff = ptr[1];
                ptr[1] = ptr[2];
                ptr[2] = buff;
            }

            var arr = Arr.AsSpan().TypeCast<int, byte>();

            for (var i = 0; i < N; i++)
                Flip(arr.Slice(i * sizeof(int), sizeof(int)));

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public void SpanFlip3()
        {
            static void Flip(Span<byte> ptr, int offset)
            {
                var buff = ptr[offset + 0];
                ptr[offset + 0] = ptr[offset + 3];
                ptr[offset + 3] = buff;
                buff = ptr[offset + 1];
                ptr[offset + 1] = ptr[offset + 2];
                ptr[offset + 2] = buff;
            }

            var arr = new byte[N * sizeof(int)];
            Buffer.BlockCopy(Arr, 0, arr, 0, N * sizeof(int));

            for (var i = 0; i < N; i++)
                Flip(arr, i * sizeof(int));

            Buffer.BlockCopy(arr, 0, Arr, 0, N * sizeof(int));
        }
    }
}
