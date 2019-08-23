using BenchmarkDotNet.Running;

namespace CopyBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<CopyBench<double>>();
            var summary = BenchmarkRunner.Run<AccessBench>();
        }
    }
}
