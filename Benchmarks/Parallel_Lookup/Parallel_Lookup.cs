using BenchmarkDotNet.Attributes;
using lowlevel_benchmark.Helpers;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lowlevel_benchmark.Benchmarks;



//[ShortRunJob]
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class Parallel_Lookup
{

#if DEBUG
	/// <summary>
	/// causes the tests to run super fast/sloppy
	/// </summary>
	[IterationSetup]
	public void Init()
	{

	}
#endif

	private const int LOOKUPTABLES = 100;
	/// <summary>
	/// how many data
	/// </summary>
	private const int COUNT = 100000;

	/// <summary>
	/// a copy of all data is stored here
	/// </summary>
	private Dictionary<long, Data> _allData_Dict = new(COUNT);
	/// <summary>
	/// a copy of all data is stored here
	/// </summary>
	private ConcurrentDictionary<long, Data> _allData_cDict = new(1, COUNT);
	/// <summary>
	/// a copy of all data is stored here
	/// </summary>
	private List<Data> _allData_List = new(COUNT);
	private Data[] _allData_Array = new Data[COUNT];
	private ArraySegment<Data> _allData_ArraySegment;


	//private int[] _allKeys;

	/// <summary>
	/// a lookup table is a randomized arangement of all keys.  used to obtain keys simulating an application random access
	/// </summary>
	private List<int[]> _lookupTables=new List<int[]>(LOOKUPTABLES);

	/// <summary>
	/// sum of all keys.   used to verify algos query all data properly
	/// </summary>
	private int _checkSum;

	private int zz_helper_LookupCounter = 0;
	/// <summary>
	/// helper to obtain all keys (indexes) in a random order.  used to obtain keys simulating an application random access
	/// </summary>
	/// <returns></returns>
	private int[] zz_HELPER_GetLookupTable()
	{

		//var index = __.Rand.Next(_lookupTables.Count);
		var index = (zz_helper_LookupCounter++) % _lookupTables.Count;
		return _lookupTables[index];
	}
	[GlobalSetup]
	public void Setup()
	{


		//create lookup tables for random access
		var basicLookupTable = new int[COUNT];
		for(var i = 0; i < COUNT; i++)
		{
			basicLookupTable[i] = i;
			_allData_List.Add(default);
		}
		for(var i = 0; i < LOOKUPTABLES; i++)
		{
			basicLookupTable._Randomize();
			var newTable = new int[COUNT];
			basicLookupTable.CopyTo(newTable, 0);
			_lookupTables.Add(newTable);
		}
		basicLookupTable._Randomize();

		//create data stores, in a random order
		for (var i = 0; i < basicLookupTable.Length; i++)
		{
			var key = basicLookupTable[i];
			var data = new Data(key);
			_allData_Dict.Add(key, data);
			var result = _allData_cDict.TryAdd(key, data);
			__ERROR.Throw(result);
			_allData_List[key] = data;
			_allData_Array[key] = data;
			_checkSum += (int)data.key;
		}
		_allData_ArraySegment = new ArraySegment<Data>(_allData_Array);

		//verfiy all rows are filled
		for (var i = 0; i < _allData_Array.Length; i++)
		{
			__ERROR.Throw(_allData_Array[i].isInit == true);

		}
	}


	/// <summary>
	/// our baseline.  lookup all data in random access from an array.
	/// </summary>
	/// <returns></returns>
	[Benchmark(Baseline = true)]
	public long Sequential_Array()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_Array[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	/// <summary>
	/// our baseline.  lookup all data in random access from an array.
	/// </summary>
	/// <returns></returns>
	[Benchmark]
	public long Sequential_Span()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		Span<Data> span = _allData_Array;

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)span[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public long Sequential_ArraySegment()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_ArraySegment[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public long Sequential_List()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_List[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public long Sequential_ListUnsafe()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_allData_List);

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)span[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public long Sequential_Dict()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_Dict[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}


	[Benchmark]
	public long Sequential_CDict()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_cDict[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public long Sequential_Dict_TryGet()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			var result = _allData_Dict.TryGetValue(key, out var data);			
			checkSum += (int)data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}


	[Benchmark]
	public long Sequential_CDict_TryGet()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			var result = _allData_Dict.TryGetValue(key, out var data);
			checkSum += (int)data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	/// <summary>
	/// our baseline.  lookup all data in random access from an array.
	/// </summary>
	/// <returns></returns>
	[Benchmark]
	public async Task<long> Sequential_Array_Task()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_Array[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	/// <summary>
	/// our baseline.  lookup all data in random access from an array.
	/// </summary>
	/// <returns></returns>
	[Benchmark]
	public async Task<long> Sequential_ArraySegment_Task()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_ArraySegment[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public async Task<long> Sequential_List_Task()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_List[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public async Task<long> Sequential_Dict_Task()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_Dict[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}


	[Benchmark]
	public async Task<long> Sequential_CDict_Task()
	{


		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		for (var i = 0; i < lookupTable.Length; i++)
		{
			var key = lookupTable[i];
			checkSum += (int)_allData_cDict[key].key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public async Task<long> Parallel_Array()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)_allData_Array[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});




		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public async Task<long> Parallel_Span()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;
			Span<Data> span = _allData_Array;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)span[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});




		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}


	[Benchmark]
	public async Task<long> Parallel_ArraySegment()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)_allData_ArraySegment[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public async Task<long> Parallel_List()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)_allData_List[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public async Task<long> Parallel_List_Unsafe()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();


		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_allData_List);

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)span[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public async Task<long> Parallel_Dict()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)_allData_Dict[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public async Task<long> Parallel_CDict()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				batchCheckSum += (int)_allData_cDict[key].key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

	[Benchmark]
	public async Task<long> Parallel_Dict_TryGet()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				var result = _allData_Dict.TryGetValue(key, out var data);
				batchCheckSum += (int)data.key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}
	[Benchmark]
	public async Task<long> Parallel_CDict_TryGet()
	{
		int checkSum = 0;
		var lookupTable = zz_HELPER_GetLookupTable();

		await P2.RangeForEachAsync_Span(0, lookupTable.Length, (pair) => {
			var (start, length) = pair;
			var end = start + length;
			var batchCheckSum = 0;

			for (var i = start; i < end; i++)
			{
				var key = lookupTable[i];
				var result = _allData_Dict.TryGetValue(key, out var data);
				batchCheckSum += (int)data.key;
			}
			Interlocked.Add(ref checkSum, batchCheckSum);


			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksum failure");


		return checkSum;
	}

}

