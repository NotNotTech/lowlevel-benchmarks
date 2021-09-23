

using BenchmarkDotNet.Running;

namespace parallel_work_gc_benchmark;

public class Program
{
	public static void Main(string[] args)
	{

		//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new BenchmarkDotNet.Configs.DebugInProcessConfig());
#if DEBUG
		var benchmark = new Benchmark();
		benchmark.Serial_Default();
#endif


		var summary = BenchmarkRunner.Run<Benchmark>();
	}
}
