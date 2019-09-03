using BenchmarkDotNet.Running;

namespace CopyBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<CopyBench<double>>();
            //var summary = BenchmarkRunner.Run<AccessBench>();
            //var summary = BenchmarkRunner.Run<RandomAccess>();
            //var summary = BenchmarkRunner.Run<Intrinsics<double>>();
            var summary = BenchmarkRunner.Run<ImageBench>();

        }
    }
}
