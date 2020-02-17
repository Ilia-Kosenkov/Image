using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace CopyBenchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    public class CopyBench<T> where T : unmanaged
    {
        public static int SizeOfT { get; } = Unsafe.SizeOf<T>();

        [Params(64_000, 111_119, 1_000_000, 1_024 * 1_024 * 16)]
        public int N;

        public Random R;
        public T[] Data;
        public T[] SrcData;
        public byte[] TargetData;



        [GlobalSetup]
        public void GlobalSetup()
        {
            R = new Random();
            Data = new T[N];
            var buff = new byte[N * SizeOfT];
            R.NextBytes(buff);
            Buffer.BlockCopy(buff, 0, Data, 0, buff.Length);

        }

        [IterationSetup]
        public void IterSetup()
        {
            SrcData = new T[N];
            TargetData = new byte[N * SizeOfT];
            Buffer.BlockCopy(Data, 0, SrcData, 0, N * SizeOfT);
        }

        [Benchmark(Baseline = true, Description = nameof(Buffer.BlockCopy))]
        public void Buffer_BlockCopy()
        {
            Buffer.BlockCopy(SrcData, 0, TargetData, 0, SrcData.Length * SizeOfT);
        }

        [Benchmark(Description = nameof(Unsafe.Write))]
        public unsafe void ElementWise_Unsafe_Write()
        {
            Span<byte> span = TargetData;
            fixed (void* ptr = span)
                for (var i = 0; i < N; i++)
                {
                    var loc = Unsafe.Add<T>(ptr, i);
                    Unsafe.Write(loc, SrcData[i]);
                }
        }

        [Benchmark(Description = nameof(Unsafe.WriteUnaligned))]
        public unsafe void ElementWise_Unsafe_WriteUnaligned()
        {
            Span<byte> span = TargetData;
            fixed(void* ptr = span)
                for (var i = 0; i < N; i++)
                {
                    var loc = Unsafe.Add<T>(ptr, i);
                    Unsafe.WriteUnaligned(loc, SrcData[i]);
                }
        }

        [Benchmark(Description = nameof(Unsafe.CopyBlock))]
        public unsafe void Unsafe_CopyBlock_F()
        {
            fixed(void* src = &SrcData[0])
                fixed(void* tar = &TargetData[0])
                    Unsafe.CopyBlock(tar, src, (uint) TargetData.Length);
        }

        [Benchmark(Description = nameof(Unsafe.CopyBlockUnaligned))]
        public unsafe void Unsafe_CopyBlockUnaligned_F()
        {
            fixed (void* src = &SrcData[0])
            fixed (void* tar = &TargetData[0])
                Unsafe.CopyBlockUnaligned(tar, src, (uint)TargetData.Length);
        }

        [Benchmark(Description = nameof(Buffer.MemoryCopy))]
        public unsafe void Buffer_MemoryCopy_F()
        {
            fixed (void* src = &SrcData[0])
            fixed (void* tar = &TargetData[0])
                Buffer.MemoryCopy(src, tar, TargetData.Length, TargetData.Length);
        }
    }
}
