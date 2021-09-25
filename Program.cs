

using BenchmarkDotNet.Running;
using lowlevel_benchmark.Benchmarks;
using lowlevel_benchmark.Helpers;

namespace lowlevel_benchmark;

public class Program
{
	public static void Main(string[] args)
	{

		//var newArgs = new string[args.Length + 1];
		//Array.Copy(args, 0, newArgs, 1, args.Length);
		//newArgs[0] = "0";
		//args = newArgs;


		//var args2 = string[]{"0"} with args;

		//args = args.Prepend("0").ToArray();

		//var x =new [] { "0" };


		//	Array.Copy
		//args =  args.j args.Prepend("0");
		//args ={"0",...args }
#if DEBUG











		//for (var i = 0; i < 100000; i++)
		//{
		//	var startInclusive = __.Rand.Next(0, 100);
		//	var length = __.Rand.Next(0, 100);
		//	var maxParallel = __.Rand.Next(1, Environment.ProcessorCount * 2);
		//	var maxBatchSize = __.Rand.Next(1, 100);
		//	var minBatchSize = __.Rand.Next(0, 100);
		//	P2.RangeFor(startInclusive, length,, maxBatchSize, minBatchSize);
		//}













		//var bm = new Collections_Threaded();
		//bm.Setup();


		//bm.Sequential_List_Read_Linq();
		//bm.Parallel_List_Read_PLinq();
		//bm.P2_RangeFor();

		//await bm.Parallel_List_Read_AsyncTask();
		//await bm.Parallel_List_Read_AsyncValueTask();

		//var benchmark = new Benchmark();
		//benchmark.Serial_Default();
		var summary = BenchmarkRunner.Run<Collections_Threaded>(new BenchmarkDotNet.Configs.DebugInProcessConfig());
		//var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
		var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
	}
}
