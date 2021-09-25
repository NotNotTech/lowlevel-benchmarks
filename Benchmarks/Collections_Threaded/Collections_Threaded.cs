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
public class Collections_Threaded
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

	private const int COUNT = 10000;
	private const int CHUNK_SIZE = 1000;
	private Dictionary<long, Data> _allData_Dict = new(COUNT);
	private ConcurrentDictionary<long, Data> _allData_cDict = new(1, COUNT);
	private List<Data> _allData_List = new(COUNT);
	private long[] _allKeys;
	//private long[] _startingKeys;
	//private long[] _addingKeys;
	//private const float ADDKEYS_PERCENT = 0.5f;
	private long _checkSum;

	private Dictionary<long, Data> _add_Dict = new(COUNT);
	private ConcurrentDictionary<long, Data> _add_cDict = new(1, COUNT);
	private List<Data> _add_List = new(COUNT);
	private List<Data> _blank_List = new(COUNT);


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
			_allData_List.Add(new Data(key));
			_blank_List.Add(default);
		}


		////create a subset of keys for adding vs those already started
		//int addAmount =(int)( COUNT * ADDKEYS_PERCENT);
		//_startingKeys = new long[COUNT -addAmount];
		//_addingKeys = new long[addAmount];
		//Array.Copy(_allKeys,_startingKeys,_startingKeys.Length);
		//Array.Copy(_allKeys, _startingKeys.Length, _addingKeys, 0, _addingKeys.Length);
	}


	public void Cleanup()
	{
		_add_Dict.Clear();
		_add_cDict.Clear();
		_add_List.Clear();
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


	[Benchmark]
	public long Sequential_List_Read()
	{
		long checkSum = 0;
		for (var i = 0; i < _allKeys.Length; i++)
		{
			var data = _allData_List[i];
			__ERROR.Throw(data.key == _allKeys[i]);
			checkSum += data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Sequential_List_Add()
	{
		long checkSum = 0;

		for (var i = 0; i < _allKeys.Length; i++)
		{
			var key = _allKeys[i];

			var newData = new Data(key);
			_add_List.Add(newData);

			var data = _add_List[i];
			__ERROR.Throw(data.key == key);

			checkSum += data.key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public long Sequential_List_AddRemove()
	{
		long checkSum = 0;

		for (var i = 0; i < _allKeys.Length; i++)
		{
			var key = _allKeys[i];
			var newData = new Data(key);

			_blank_List[i] = newData;


			var data = _blank_List[i];
			__ERROR.Throw(data.key == key);

			checkSum += data.key;

			_blank_List[i] = default;

		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public long Parallel_List_Read()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var data = _allData_List[i];
			__ERROR.Throw(data.key == _allKeys[i]);
			Interlocked.Add(ref checkSum, data.key);
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public long Parallel_List_Add()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			var key = _allKeys[i];

			var newData = new Data(key);
			_blank_List[i] = newData;

			var data = _blank_List[i];
			__ERROR.Throw(data.key == key);

			Interlocked.Add(ref checkSum, data.key);
		});
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public long Parallel_List_AddRemove()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{

			var key = _allKeys[i];
			var newData = new Data(key);

			_blank_List[i] = newData;


			var data = _blank_List[i];
			__ERROR.Throw(data.key == key);

			Interlocked.Add(ref checkSum, data.key);

			_blank_List[i] = default;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		Cleanup();
		return checkSum;
	}
	[Benchmark(Baseline = true)]
	public long Parallel_NoWork_InterlockedAdd()
	{
		long checkSum = 0;

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			Interlocked.Add(ref checkSum, i);
		});

		//__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}
	/// <summary>
	/// experiment to see how adding serial may be faster than a threaded interlocked
	/// </summary>
	[Benchmark]
	public long Parallel_NoWork_AddSerial()
	{
		long checkSum = 0;

		using var checkSumOwner = SpanOwner<long>.Allocate(_allKeys.Length);
		var checkSumArray = checkSumOwner.DangerousGetArray().Array;
		var checkSumSpan = checkSumOwner.Span;
		Parallel.For(0, _allKeys.Length, (i) =>
		{
			checkSumArray[i] = i;
			//Interlocked.Add(ref checkSum, i);
		});


		//do sum in single thread
		for (var i = 0; i < _allKeys.Length; i++)
		{
			checkSum += checkSumSpan[i];
		}

		//__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	/// <summary>
	/// experiment to see how adding serial may be faster than a threaded interlocked
	/// </summary>
	[Benchmark]
	public long Parallel_NoWork_AddSerial_ThreadLocal()
	{

		ThreadLocal<long> checkSumThreadlocal = new ThreadLocal<long>(true);

		Parallel.For(0, _allKeys.Length, (i) =>
		{
			checkSumThreadlocal.Value += i;
		});


		long checkSum = 0;
		//do sum in single thread
		foreach (var checkSumLocal in checkSumThreadlocal.Values)
		{
			checkSum += checkSumLocal;
		}

		//__ERROR.Throw(checkSum == _checkSum, "checksums don't match");
		checkSumThreadlocal.Dispose();

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public async Task<long> Parallel_List_Read_AsyncTask()
	{
		long checkSum = 0;

		await Parallel.ForEachAsync(_allData_List, async (data, cancelToken) =>
		{


			Interlocked.Add(ref checkSum, data.key);

			//return ValueTask.CompletedTask;
		});


		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public async Task<long> Parallel_List_Read_AsyncValueTask()
	{
		long checkSum = 0;

		await Parallel.ForEachAsync(_allData_List, (data, cancelToken) =>
		{


			Interlocked.Add(ref checkSum, data.key);

			return ValueTask.CompletedTask;
		});


		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public async Task<long> Sequential_List_Read_AsyncAwaitTask()
	{
		long checkSum = 0;

		for (var i = 0; i < _allData_List.Count; i++)
		{
			var index = i;
			var task = Task.Run(() =>
			{
				var data = _allData_List[index];
				Interlocked.Add(ref checkSum, data.key);
				return ValueTask.CompletedTask;
			});
			await task;

		}



		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}



	List<Task> tasksCurrent = new(COUNT);
	List<Task> tasksOld = new(COUNT);


	[Benchmark]
	public async Task<long> Parallel_List_Read_AsyncAwaitTask()
	{
		long checkSum = 0;

		for (var i = 0; i < _allData_List.Count; i++)
		{
			var index = i;
			var task = Task.Run(() =>
			{
				var data = _allData_List[index];
				Interlocked.Add(ref checkSum, data.key);
				return ValueTask.CompletedTask;
			});
			tasksCurrent.Add(task);


			if (tasksCurrent.Count >= Environment.ProcessorCount)
			{
				if (tasksOld.Count > 0)
				{
					await Task.WhenAll(tasksOld);
					tasksOld.Clear();
					var temp = tasksOld;
					tasksOld = tasksCurrent;
					tasksCurrent = temp;
				}

			}
		}
		await Task.WhenAll(tasksOld);
		await Task.WhenAll(tasksCurrent);
		tasksOld.Clear();
		tasksCurrent.Clear();



		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public long Sequential_List_Read_Linq()
	{
		long checkSum = 0;

		foreach (var key in _allData_List.Select((data) => data.key))
		{
			checkSum += key;
		}
		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;


	}

	[Benchmark]
	public long Parallel_List_Read_PLinq()
	{
		long checkSum = _allData_List.AsParallel().Select((data) => data.key).Aggregate((acc, item) => acc + item);


		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}



	[Benchmark]
	public long P2_RangeFor()
	{
		long checkSum = 0;


		P2.RangeFor(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				Interlocked.Add(ref checkSum, data.key);
			}
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public async Task<long> P2_RangeForEachAsync()
	{
		long checkSum = 0;


		await P2.RangeForEachAsync(0, _allKeys.Length, async (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				Interlocked.Add(ref checkSum, data.key);
			}
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public async Task<long> P2_RangeForEachAsync_ValueTask()
	{
		long checkSum = 0;


		await P2.RangeForEachAsync(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				Interlocked.Add(ref checkSum, data.key);
			}
			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public async Task<long> P2_RangeActionBlock()
	{
		long checkSum = 0;


		await P2.RangeActionBlock(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				Interlocked.Add(ref checkSum, data.key);
			}
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public async Task<long> P2_RangeActionBlock_Task()
	{
		long checkSum = 0;


		await P2.RangeActionBlock_Task(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				Interlocked.Add(ref checkSum, data.key);
			}
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public async Task<long> P2_RangeAsync()
	{
		long checkSum = 0;


		await P2.RangeAsync(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				Interlocked.Add(ref checkSum, data.key);
			}
			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public long P2_RangeFor_SumInterlock()
	{
		long checkSum = 0;


		P2.RangeFor(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public async Task<long> P2_RangeForEachAsync_SumInterlock()
	{
		long checkSum = 0;


		await P2.RangeForEachAsync(0, _allKeys.Length, async (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public async Task<long> P2_RangeForEachAsync_ValueTask_SumInterlock()
	{
		long checkSum = 0;


		await P2.RangeForEachAsync(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public async Task<long> P2_RangeActionBlock_SumInterlock()
	{
		long checkSum = 0;


		await P2.RangeActionBlock(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public async Task<long> P2_RangeActionBlock_TaskExplicit_SumInterlock()
	{
		long checkSum = 0;


		await P2.RangeActionBlock_Task(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}


	[Benchmark]
	public async Task<long> P2_RangeAsync_SumInterlock()
	{
		long checkSum = 0;


		await P2.RangeAsync(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

	[Benchmark]
	public async Task<long> P2_RangeAsync_Task_SumInterlock()
	{
		long checkSum = 0;


		await P2.RangeAsync_Task(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			return Task.CompletedTask;
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}
	[Benchmark]
	public async Task<long> P2_RangeAsync_Task_SumInterlock2()
	{
		long checkSum = 0;


		await P2.RangeAsync_Task(0, _allKeys.Length, (batch) =>
		{
			var (start, length) = batch;
			var end = start + length;

			long sum = 0;
			for (var i = start; i < end; i++)
			{
				var data = _allData_List[i];
				__ERROR.Throw(data.key == _allKeys[i]);
				sum += data.key;
			}
			Interlocked.Add(ref checkSum, sum);
			return Task.CompletedTask;
			//return ValueTask.CompletedTask;
		});

		__ERROR.Throw(checkSum == _checkSum, "checksums don't match");

		Cleanup();
		return checkSum;
	}

}

