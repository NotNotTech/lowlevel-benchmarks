

using BenchmarkDotNet.Running;
using lowlevel_benchmark.Benchmarks;
using lowlevel_benchmark.Helpers;

namespace lowlevel_benchmark;

public class Program
{
	public static void Main(string[] args)
	{
#if DEBUG



		var testInt = new NullTesting<int>();

		var result = testInt.TryGet(0, out var val);

		__ERROR.Assert(val != null);




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




public class NullTesting<TValue>
{
	private TValue?[] storage = new TValue?[100];

	public bool TryGet(int index, out TValue? value)
	{
		value = storage[index];
		return value != null;
	}
}



public class NullTesting_Struct<TValue> where TValue : struct
{
	private Nullable<TValue>[] storage = new Nullable<TValue>[100];

	public bool TryGet(int index, out TValue value)
	{
		var result = storage[index];
		value = result.Value;
		return result.HasValue;
	}
}


public class NullTesting_Tuple<TValue> where TValue : struct
{
	private (bool hasValue,TValue value)[] storage = new (bool valid, TValue value)[100];
	public bool TryGet(int index, out TValue value)
	{
		var result = storage[index];
		value = result.value;
		return result.hasValue;
	}
	public ref TValue GetRef(int index)
	{
		return ref storage[index].value;
	}
}

