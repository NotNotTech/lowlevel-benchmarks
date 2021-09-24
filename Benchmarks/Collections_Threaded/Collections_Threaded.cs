using BenchmarkDotNet.Attributes;
using lowlevel_benchmark.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lowlevel_benchmark.Benchmarks;



//[ShortRunJob]
[MemoryDiagnoser]
public class Collections_Threaded
{
	private const int COUNT = 10000;
	private const int CHUNK_SIZE = 1000;
	private Dictionary<long, Data> _allData_Dict = new(0);
	private ConcurrentDictionary<long, Data> _allData_cDict = new(1, COUNT);
	private long[] _allKeys;
	//private long[] _startingKeys;
	//private long[] _addingKeys;
	//private const float ADDKEYS_PERCENT = 0.5f;
	private long _checkSum;

	private Dictionary<long, Data> _add_Dict = new(0);
	private ConcurrentDictionary<long, Data> _add_cDict = new(1, COUNT);


	[GlobalSetup]
	public void Setup()
	{
		var set = new HashSet<long>();
		while (set.Count < COUNT)
		{
			set.Add(__.Rand.NextInt64());
		}
		_allKeys = set.ToArray();
		foreach (var key in _allKeys)
		{
			_checkSum += key;
			_allData_Dict.Add(key, new Data(key));
			_allData_cDict.TryAdd(key, new Data(key));
		}

		////create a subset of keys for adding vs those already started
		//int addAmount =(int)( COUNT * ADDKEYS_PERCENT);
		//_startingKeys = new long[COUNT -addAmount];
		//_addingKeys = new long[addAmount];
		//Array.Copy(_allKeys,_startingKeys,_startingKeys.Length);
		//Array.Copy(_allKeys, _startingKeys.Length, _addingKeys, 0, _addingKeys.Length);
	}
	[IterationSetup]
	public void Init()
	{

	}

	public void Cleanup()
	{
		_add_Dict.Clear();
		_add_cDict.Clear();
	}

	//[Benchmark(Baseline = true)]
	[Benchmark]
	public long Sequential_Dict_Read()
	{
		long checkSum = 0;
		foreach (var key in _allKeys)
		{
			var result = _allData_Dict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);
			checkSum += data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Sequential_Dict_Add()
	{
		long checkSum = 0;

		foreach (var key in _allKeys)
		{
			var newData = new Data(key);
			_add_Dict.Add(key, newData);
			var result = _add_Dict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);

			checkSum += data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public long Sequential_Dict_AddRemove()
	{
		long checkSum = 0;

		foreach (var key in _allKeys)
		{
			var newData = new Data(key);
			_add_Dict.Add(key, newData);
			//_add_Dict.Add(key, newData);
			var result = _add_Dict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);

			checkSum += data.key;

			result = _add_Dict.Remove(key);
			__ERROR.Throw(result);
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public long Sequential_CDict_Read()
	{
		long checkSum = 0;
		foreach (var key in _allKeys)
		{
			var result = _allData_cDict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);
			checkSum += data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		return checkSum;
	}

	[Benchmark]
	public long Sequential_CDict_Add()
	{
		long checkSum = 0;

		foreach (var key in _allKeys)
		{
			var newData = new Data(key);
			var result = _add_cDict.TryAdd(key, newData);
			__ERROR.Throw(result);
			result = _add_cDict.TryGetValue(key, out var data);
			__ERROR.Throw(result);

			checkSum += data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Sequential_CDict_AddRemove()
	{
		long checkSum = 0;

		foreach (var key in _allKeys)
		{
			var newData = new Data(key);
			var result = _add_cDict.TryAdd(key, newData);
			__ERROR.Throw(result);
			//result = _add_cDict.TryAdd(key, newData);
			//__ERROR.Throw(result && data.key == key);
			result = _add_cDict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);

			checkSum += data.key;
			result = _add_cDict.TryRemove(key, out data);
			__ERROR.Throw(result && data.key == key);
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public long Parallel_Dict_Read()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];
			var result = _allData_Dict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);

			Interlocked.Add(ref checkSum, data.key);
		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Parallel_CDict_Read()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];
			var result = _allData_cDict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);

			Interlocked.Add(ref checkSum, data.key);
		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Parallel_Dict_Add()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];
			var newData = new Data(key);
			lock (_add_Dict)
			{
				_add_Dict.Add(key, newData);
			}
			{

				var result = _add_Dict.TryGetValue(key, out var data);
				__ERROR.Throw(result && data.key == key);

				Interlocked.Add(ref checkSum, data.key);
			}

		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();

		return checkSum;
	}
	[Benchmark]
	public long Parallel_CDict_Add()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];
			var newData = new Data(key);
			//lock (_add_cDict)
			{
				var result = _add_cDict.TryAdd(key, newData);
				__ERROR.Throw(result);
			}
			{
				var result = _add_cDict.TryGetValue(key, out var data);
				__ERROR.Throw(result && data.key == key);
				Interlocked.Add(ref checkSum, data.key);
			}
		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Parallel_Dict_AddRemove()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];
			var newData = new Data(key);
			////add
			lock (_add_Dict)
			{
				_add_Dict.Add(key, newData);
				var result = _add_Dict.TryGetValue(key, out var data);
				__ERROR.Throw(result);				
			}
			//read, unlocked 
			{

				var result = _add_Dict.TryGetValue(key, out var data);
				__ERROR.Throw(result && data.key == key);

				Interlocked.Add(ref checkSum, data.key);
			}
			//remove
			lock (_add_Dict)
			{
				var result = _add_Dict.Remove(key);
				__ERROR.Throw(result);
			}

		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();

		return checkSum;
	}
	[Benchmark]
	public long Parallel_CDict_AddRemove()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];
			var newData = new Data(key);

			var result = _add_cDict.TryAdd(key, newData);
			__ERROR.Throw(result);
			//result = _add_cDict.TryAdd(key, newData);
			//__ERROR.Throw(result && data.key == key);
			result = _add_cDict.TryGetValue(key, out var data);
			__ERROR.Throw(result && data.key == key);
			result = _add_cDict.TryRemove(key, out data);
			__ERROR.Throw(result && data.key == key);

			Interlocked.Add(ref checkSum, data.key);

		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
}


