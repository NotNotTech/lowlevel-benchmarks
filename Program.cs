

using BenchmarkDotNet.Running;
using lowlevel_benchmark.Benchmarks;
using lowlevel_benchmark.Helpers;

namespace lowlevel_benchmark;

public class Program
{
	public static void Main(string[] args)
	{
#if DEBUG

		//run in debug mode (can hit breakpoints in VS)
		var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new BenchmarkDotNet.Configs.DebugInProcessConfig());		
//run a specific benchmark
		//var summary = BenchmarkRunner.Run<Parallel_Lookup>(new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
		var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
	}
}
