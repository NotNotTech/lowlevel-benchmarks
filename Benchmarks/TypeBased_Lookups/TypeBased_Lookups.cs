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
		if (!storage.TryGetValue(typeof(T), out var index))
		{
			index = indexCounter++;
			storage.Add(typeof(T), index);
		}
		return index;
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
			toReturn += GlobalLookup.Get<int>();
			toReturn += GlobalLookup.Get<float>();
			toReturn += GlobalLookup.Get<bool>();
			toReturn += GlobalLookup.Get<long>();
			toReturn += GlobalLookup.Get<byte>();
			toReturn += GlobalLookup.Get<short>();
			toReturn += GlobalLookup.Get<double>();
			toReturn += GlobalLookup.Get<string>();
			toReturn += GlobalLookup.Get<object>();
		}

		return toReturn;
	}
	[Benchmark]
	public int InstanceLookupTest()
	{
		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			toReturn += instanceLookup.Get<int>();
			toReturn += instanceLookup.Get<float>();
			toReturn += instanceLookup.Get<bool>();
			toReturn += instanceLookup.Get<long>();
			toReturn += instanceLookup.Get<byte>();
			toReturn += instanceLookup.Get<short>();
			toReturn += instanceLookup.Get<double>();
			toReturn += instanceLookup.Get<string>();
			toReturn += instanceLookup.Get<object>();
		}
		return toReturn;
	}
}