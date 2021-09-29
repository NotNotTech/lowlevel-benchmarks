using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lowlevel_benchmark.Benchmarks;


public abstract class GlobalLookup
{
	protected static int indexCounter;

	public static int Get<T>()
	{
		return GlobalLookup<T>.Get();
	}
}
public class GlobalLookup<T> : GlobalLookup
{
	private static int index = Interlocked.Increment(ref indexCounter);
	public static int Get()
	{
		return index;
	}

}


public class InstanceLookup
{
	private int indexCounter;

	private Dictionary<Type, int> storage = new();

	public int Get<T>()
	{
		ref var toReturn = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(storage, typeof(T), out var exists);
		if (!exists)
		{
			toReturn = indexCounter++;
		}
		return toReturn;

		//if (!storage.TryGetValue(typeof(T), out var index))
		//{
		//	index = indexCounter++;
		//	storage.Add(typeof(T), index);
		//}
		//return index;
	}

}

public class TypeBased_Lookups
{
	private int LOOPCOUNT = 10000;
	private InstanceLookup instanceLookup = new();

	[Benchmark]
	public int GlobalLookupTest()
	{
		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			//toReturn += GlobalLookup.Get<int>();
			//toReturn += GlobalLookup.Get<float>();
			//toReturn += GlobalLookup.Get<bool>();
			//toReturn += GlobalLookup.Get<long>();
			//toReturn += GlobalLookup.Get<byte>();
			//toReturn += GlobalLookup.Get<short>();
			//toReturn += GlobalLookup.Get<double>();
			//toReturn += GlobalLookup.Get<string>();
			//toReturn += GlobalLookup.Get<object>();
			toReturn += GlobalLookup.Get<GlobalLookup<int>>();
			toReturn += GlobalLookup.Get<GlobalLookup<string>>();
			toReturn += GlobalLookup.Get<GlobalLookup<object>>();
			toReturn += GlobalLookup.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += GlobalLookup.Get<InstanceLookup>();
			toReturn += GlobalLookup.Get<TypeBased_Lookups>();
			toReturn += GlobalLookup.Get<System.Threading.Thread>();
			toReturn += GlobalLookup.Get<System.Collections.ArrayList>();
		}

		return toReturn;
	}
	[Benchmark]
	public int InstanceLookupTest()
	{
		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			//toReturn += instanceLookup.Get<int>();
			//toReturn += instanceLookup.Get<float>();
			//toReturn += instanceLookup.Get<bool>();
			//toReturn += instanceLookup.Get<long>();
			//toReturn += instanceLookup.Get<byte>();
			//toReturn += instanceLookup.Get<short>();
			//toReturn += instanceLookup.Get<double>();
			//toReturn += instanceLookup.Get<string>();
			//toReturn += instanceLookup.Get<object>();
			toReturn += instanceLookup.Get<GlobalLookup<int>>();
			toReturn += instanceLookup.Get<GlobalLookup<string>>();
			toReturn += instanceLookup.Get<GlobalLookup<object>>();
			toReturn += instanceLookup.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += instanceLookup.Get<InstanceLookup>();
			toReturn += instanceLookup.Get<TypeBased_Lookups>();
			toReturn += instanceLookup.Get<System.Threading.Thread>();
			toReturn += instanceLookup.Get<System.Collections.ArrayList>();
		}
		return toReturn;
	}
}