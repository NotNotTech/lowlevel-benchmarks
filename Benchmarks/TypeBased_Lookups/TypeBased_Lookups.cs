using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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


[SimpleJob(runStrategy: BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 1)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class LookupBenchmark
{
	private int LOOPCOUNT = 10000;
	public static class StaticTypeLocal<TValue>
	{
		private static class TypeStore<T>
		{
			internal static TValue Value;
		}

		public static TValue Get<T>() => TypeStore<T>.Value;

		public static void Set<T>(TValue value) => TypeStore<T>.Value = value;
	}

	public readonly struct InstanceTypeLocalDictionary<TValue>
	{
		private readonly Dictionary<Type, TValue> _storage = new(10);

		public TValue Get<T>() => _storage[typeof(T)];

		public void Set<T>(TValue value) => _storage[typeof(T)] = value;
	}



	public struct InstanceTypeLocalArray<TValue>
	{
		private static volatile int TypeIndex = -1;


		private static class TypeSlot<T>
		{
			internal static readonly int Index = Interlocked.Increment(ref TypeIndex);
		}

		/// <summary>
		/// A small inefficiency:  will have 1 slot for each TType ever used for a TypeLocal call, regardless of if it's used in this instance or not
		/// </summary>
		private TValue[] _storage;

		public InstanceTypeLocalArray()
		{
			_storage = new TValue[Math.Max(100, TypeIndex + 1)];
		}

		private TValue[] EnsureStorageCapacity<T>()
		{
			if (TypeSlot<T>.Index >= _storage.Length)
				Array.Resize(ref _storage, TypeSlot<T>.Index + 1);

			return _storage;
		}

		public void Set<T>(TValue value)
		{
			//Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureStorageCapacity<T>()), TypeSlot<T>.Index) = value;
			var storage = EnsureStorageCapacity<T>();
			storage[TypeSlot<T>.Index] = value;
		}

		public TValue Get<T>()
		{
			//return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureStorageCapacity<T>()), TypeSlot<T>.Index);
			//return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_storage), TypeSlot<T>.Index);
			//unsafe
			//{
			//	fixed (TValue* pStorage = &_storage[0])
			//	{

			//	}
			//}

			return _storage[TypeSlot<T>.Index];
		}

		public ref TValue GetRef<T>()
		{
			//return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_storage), TypeSlot<T>.Index);
			return ref _storage[TypeSlot<T>.Index];
		}
	}


	/// <summary>
	/// efficiently get/set a value for a given type. 
	/// <para>similar use as a <see cref="ThreadLocal{T}"/></para>
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public struct TypeLocal<TValue>
	{
		private static volatile int _typeCounter = -1;


		private static class TypeSlot<TType>
		{
			internal static readonly int _index = Interlocked.Increment(ref _typeCounter);
		}

		/// <summary>
		/// A small inefficiency:  will have 1 slot for each TType ever used for a TypeLocal call, regardless of if it's used in this instance or not
		/// </summary>
		private (bool hasValue, TValue value)[] _storage;

		private Func<Type, TValue> _valueFactory;

		public TypeLocal()
		{
			throw new Exception("use other ctor");
		}

		public TypeLocal(Func<Type, TValue> valueFactory)
		{
			_storage = new (bool hasValue, TValue value)[Math.Max(10, _typeCounter + 1)];
			_valueFactory = valueFactory;
		}

		private (bool hasValue, TValue value)[] EnsureStorageCapacity<TType>()
		{
			if (TypeSlot<TType>._index >= _storage.Length)
			{
				Array.Resize(ref _storage, (_typeCounter + 1) * 2);
			}
			return _storage;
		}

		public void Set<TType>(TValue value)
		{
			//Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureStorageCapacity<T>()), TypeSlot<T>.Index) = value;
			var storage = EnsureStorageCapacity<TType>();
			storage[TypeSlot<TType>._index] = (true, value);
		}

		public TValue Get<TType>()
		{
			//return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureStorageCapacity<T>()), TypeSlot<T>.Index);
			//return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_storage), TypeSlot<T>.Index);
			//unsafe
			//{
			//	fixed (TValue* pStorage = &_storage[0])
			//	{

			//	}
			//}

			var result = _storage[TypeSlot<TType>._index];
			if (result.hasValue == false)
			{
				if (_valueFactory == null)
				{
					throw new ArgumentNullException(typeof(TType).Name);
				}
				_storage[TypeSlot<TType>._index] = (true, _valueFactory(typeof(TType)));
				result = _storage[TypeSlot<TType>._index];
			}
			return result.value;
		}

		public ref TValue GetRef<TType>()
		{
			//return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_storage), TypeSlot<T>.Index);
			//return ref _storage[TypeSlot<TType>._index].value;

			ref var result = ref _storage[TypeSlot<TType>._index];
			if (result.hasValue == false)
			{
				if (_valueFactory == null)
				{
					throw new ArgumentNullException(typeof(TType).Name);
				}
				_storage[TypeSlot<TType>._index] = (true, _valueFactory(typeof(TType)));
				result = ref _storage[TypeSlot<TType>._index];
			}
			return ref result.value;
		}


		public bool TryGet<TType>(int index, out TValue value)
		{

			var result = _storage[TypeSlot<TType>._index];
			value = result.value;
			return result.hasValue;
		}


	}
	public struct TypeLocalSlim<TValue>
	{
		private static volatile int _typeCounter = -1;


		private static class TypeSlot<TType>
		{
			internal static readonly int _index = Interlocked.Increment(ref _typeCounter);
		}

		/// <summary>
		/// A small inefficiency:  will have 1 slot for each TType ever used for a TypeLocal call, regardless of if it's used in this instance or not
		/// </summary>
		private TValue[] _storage;

		public TypeLocalSlim()
		{
			_storage = new TValue[Math.Max(10, _typeCounter + 1)];
		}

		private TValue[] EnsureStorageCapacity<TType>()
		{
			if (TypeSlot<TType>._index >= _storage.Length)
			{
				Array.Resize(ref _storage, (_typeCounter + 1) * 2);
			}
			return _storage;
		}

		public void Set<TType>(TValue value)
		{
			//Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureStorageCapacity<T>()), TypeSlot<T>.Index) = value;
			var storage = EnsureStorageCapacity<TType>();
			storage[TypeSlot<TType>._index] = value;
		}

		public TValue Get<TType>()
		{
			//return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureStorageCapacity<T>()), TypeSlot<T>.Index);
			//return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_storage), TypeSlot<T>.Index);
			//unsafe
			//{
			//	fixed (TValue* pStorage = &_storage[0])
			//	{

			//	}
			//}

			return _storage[TypeSlot<TType>._index];
		}

		public ref TValue GetRef<TType>()
		{
			//return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_storage), TypeSlot<T>.Index);
			//return ref _storage[TypeSlot<TType>._index].value;

			return ref _storage[TypeSlot<TType>._index];
		}




	}


	private InstanceTypeLocalDictionary<int> typeLocalDictionary = new();
	private InstanceTypeLocalArray<int> typeLocalArray = new();
	private TypeLocal<int> typeLocal = new(null);
	private TypeLocalSlim<int> typeLocalSlim = new();


	[GlobalSetup]
	public void Setup()
	{
		{
			//static setup
			var count = 0;
			StaticTypeLocal<int>.Set<GlobalLookup<int>>(count++);
			StaticTypeLocal<int>.Set<GlobalLookup<string>>(count++);
			StaticTypeLocal<int>.Set<GlobalLookup<object>>(count++);
			StaticTypeLocal<int>.Set<GlobalLookup<TypeBased_Lookups>>(count++);
			StaticTypeLocal<int>.Set<InstanceLookup>(count++);
			StaticTypeLocal<int>.Set<TypeBased_Lookups>(count++);
			StaticTypeLocal<int>.Set<System.Threading.Thread>(count++);
			StaticTypeLocal<int>.Set<System.Collections.ArrayList>(count++);
		}

		{
			//dictionary setup
			var count = 0;
			typeLocalDictionary.Set<GlobalLookup<int>>(count++);
			typeLocalDictionary.Set<GlobalLookup<string>>(count++);
			typeLocalDictionary.Set<GlobalLookup<object>>(count++);
			typeLocalDictionary.Set<GlobalLookup<TypeBased_Lookups>>(count++);
			typeLocalDictionary.Set<InstanceLookup>(count++);
			typeLocalDictionary.Set<TypeBased_Lookups>(count++);
			typeLocalDictionary.Set<System.Threading.Thread>(count++);
			typeLocalDictionary.Set<System.Collections.ArrayList>(count++);
		}
		{
			//array setup
			var count = 0;
			typeLocalArray.Set<GlobalLookup<int>>(count++);
			typeLocalArray.Set<GlobalLookup<string>>(count++);
			typeLocalArray.Set<GlobalLookup<object>>(count++);
			typeLocalArray.Set<GlobalLookup<TypeBased_Lookups>>(count++);
			typeLocalArray.Set<InstanceLookup>(count++);
			typeLocalArray.Set<TypeBased_Lookups>(count++);
			typeLocalArray.Set<System.Threading.Thread>(count++);
			typeLocalArray.Set<System.Collections.ArrayList>(count++);
		}
		{
			//typelocal setup
			var count = 0;
			typeLocal.Set<GlobalLookup<int>>(count++);
			typeLocal.Set<GlobalLookup<string>>(count++);
			typeLocal.Set<GlobalLookup<object>>(count++);
			typeLocal.Set<GlobalLookup<TypeBased_Lookups>>(count++);
			typeLocal.Set<InstanceLookup>(count++);
			typeLocal.Set<TypeBased_Lookups>(count++);
			typeLocal.Set<System.Threading.Thread>(count++);
			typeLocal.Set<System.Collections.ArrayList>(count++);
		}
		{
			//typelocalSlim setup
			var count = 0;
			typeLocalSlim.Set<GlobalLookup<int>>(count++);
			typeLocalSlim.Set<GlobalLookup<string>>(count++);
			typeLocalSlim.Set<GlobalLookup<object>>(count++);
			typeLocalSlim.Set<GlobalLookup<TypeBased_Lookups>>(count++);
			typeLocalSlim.Set<InstanceLookup>(count++);
			typeLocalSlim.Set<TypeBased_Lookups>(count++);
			typeLocalSlim.Set<System.Threading.Thread>(count++);
			typeLocalSlim.Set<System.Collections.ArrayList>(count++);
		}
	}

	[Benchmark]
	public int UsingTypeLocalSlim()
	{

		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			toReturn += typeLocalSlim.Get<GlobalLookup<int>>();
			toReturn += typeLocalSlim.Get<GlobalLookup<string>>();
			toReturn += typeLocalSlim.Get<GlobalLookup<object>>();
			toReturn += typeLocalSlim.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += typeLocalSlim.Get<InstanceLookup>();
			toReturn += typeLocalSlim.Get<TypeBased_Lookups>();
			toReturn += typeLocalSlim.Get<System.Threading.Thread>();
			toReturn += typeLocalSlim.Get<System.Collections.ArrayList>();
		}
		return toReturn;

	}

	[Benchmark]
	public int UsingTypeLocal()
	{

		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			toReturn += typeLocal.Get<GlobalLookup<int>>();
			toReturn += typeLocal.Get<GlobalLookup<string>>();
			toReturn += typeLocal.Get<GlobalLookup<object>>();
			toReturn += typeLocal.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += typeLocal.Get<InstanceLookup>();
			toReturn += typeLocal.Get<TypeBased_Lookups>();
			toReturn += typeLocal.Get<System.Threading.Thread>();
			toReturn += typeLocal.Get<System.Collections.ArrayList>();
		}
		return toReturn;

	}
	[Benchmark]
	public int UsingArray()
	{

		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			toReturn += typeLocalArray.Get<GlobalLookup<int>>();
			toReturn += typeLocalArray.Get<GlobalLookup<string>>();
			toReturn += typeLocalArray.Get<GlobalLookup<object>>();
			toReturn += typeLocalArray.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += typeLocalArray.Get<InstanceLookup>();
			toReturn += typeLocalArray.Get<TypeBased_Lookups>();
			toReturn += typeLocalArray.Get<System.Threading.Thread>();
			toReturn += typeLocalArray.Get<System.Collections.ArrayList>();
		}
		return toReturn;

	}

	[Benchmark]
	public int UsingStaticClass()
	{
		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			toReturn += StaticTypeLocal<int>.Get<GlobalLookup<int>>();
			toReturn += StaticTypeLocal<int>.Get<GlobalLookup<string>>();
			toReturn += StaticTypeLocal<int>.Get<GlobalLookup<object>>();
			toReturn += StaticTypeLocal<int>.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += StaticTypeLocal<int>.Get<InstanceLookup>();
			toReturn += StaticTypeLocal<int>.Get<TypeBased_Lookups>();
			toReturn += StaticTypeLocal<int>.Get<System.Threading.Thread>();
			toReturn += StaticTypeLocal<int>.Get<System.Collections.ArrayList>();
		}
		return toReturn;
	}

	[Benchmark]
	public int UsingDictionary()
	{
		var toReturn = 0;
		for (var i = 0; i < LOOPCOUNT; i++)
		{
			toReturn += typeLocalDictionary.Get<GlobalLookup<int>>();
			toReturn += typeLocalDictionary.Get<GlobalLookup<string>>();
			toReturn += typeLocalDictionary.Get<GlobalLookup<object>>();
			toReturn += typeLocalDictionary.Get<GlobalLookup<TypeBased_Lookups>>();
			toReturn += typeLocalDictionary.Get<InstanceLookup>();
			toReturn += typeLocalDictionary.Get<TypeBased_Lookups>();
			toReturn += typeLocalDictionary.Get<System.Threading.Thread>();
			toReturn += typeLocalDictionary.Get<System.Collections.ArrayList>();
		}
		return toReturn;

	}

}
