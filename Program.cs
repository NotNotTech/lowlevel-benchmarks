

using BenchmarkDotNet.Running;
using lowlevel_benchmark.Benchmarks;

namespace lowlevel_benchmark;

public class Program
{
	public static void Main(string[] args)
	{

		//var newArgs = new string[args.Length + 1];
		//Array.Copy(args, 0, newArgs, 1, args.Length);
		//newArgs[0] = "0";
		//args = newArgs;



		//var x =new [] { "0" };


		//	Array.Copy
		//args =  args.j args.Prepend("0");
		//args ={"0",...args }
#if DEBUG

		var bm = new Collections_Threaded();
		bm.Setup();
		bm.Parallel_Dict_AddRemove();

		//var benchmark = new Benchmark();
		//benchmark.Serial_Default();
		var summary = BenchmarkRunner.Run<Collections_Threaded>(new BenchmarkDotNet.Configs.DebugInProcessConfig());
		//var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
		var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
	}
}
