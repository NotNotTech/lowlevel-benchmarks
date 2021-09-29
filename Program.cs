

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



//public class SomeClass
//{
//	private int indexCounter;
//	public int GetTypeIndex<T>()
//	{
//		var typeLocal = new TypeLocal<int>();
//		if (!typeLocal.TryGet<T>(out var index){
//			index = indexCounter++;
//			typeLocal.Set<T>(index);
//		}
//		return index;
//	}

//}

///// <summary>
///// similar to ThreadLocal, but provides a value per type.
///// </summary>
///// <typeparam name="TValue"></typeparam>
//public interface TypeLocal<TValue>
//{
//	public TValue Get<TType>();

//	public bool TryGet<TType>(out TValue value);

//	public void Set<TType>(TValue value);

//	public void Remove<TType>();

//	public bool HasValue<TType>();

//	public IEnumerable<(Type type, TValue value)> All();
//}