using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Image;

namespace CopyBenchmarks
{
    [CoreJob()]
    public class ImageBench
    {
        private Random _r;
        private IImmutableImage<int> _image;

        [Params(256, 512, 1024, 2048)]
        public int Width;

        public int Height => Width;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _r = new Random();
        }

        [IterationSetup]
        public void IterSteup()
        {
            var data = new int[Width * Height];

            for (var i = 0; i < data.Length; i++)
                data[i] = _r.Next(-(1 << 24), 1 << 24);

            _image = new Image<int>(data, Height, Width);
        }


        [Benchmark]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void TestScaleClamp()
        {
            var img = _image.CastTo<double>().Clamp(-(1 << 16), 1 << 16).Scale(0, 1 << 8);
        }
    }
}
