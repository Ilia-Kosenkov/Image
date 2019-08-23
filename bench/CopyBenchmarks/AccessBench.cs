using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using BenchmarkDotNet.Attributes;

namespace CopyBenchmarks
{
    [CoreJob()]
    public class AccessBench
    {
        public Random R;
        public int[] Arr1;
        public int[] Arr2;

        [Params(1024*1024, 8 * 1024 * 1024, 64 * 1024 * 1024)]
        public int N;

        [GlobalSetup]
        public void GlobalSetup()
        {
            R = new Random();
        }

        [IterationSetup]
        public void IterSetup()
        {
            Arr1 = new int[N];
            Arr2 = new int[N];

            for (var i = 0; i < N; i++)
                Arr1[i] = Arr2[i] = R.Next();


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
                Arr1[i] = Flip(Arr1[i]);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public unsafe void SpanFlip()
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

            fixed (int* sptr = Arr2)
            {
                var p = (byte*) sptr;
                for (var i = 0; i < N; i++)
                {
                   Flip(p + i * sizeof(int));
                }
            }
        }
    }
}
