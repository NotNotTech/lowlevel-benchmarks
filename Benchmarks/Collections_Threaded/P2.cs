﻿using lowlevel_benchmark.Helpers;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace lowlevel_benchmark.Benchmarks;

public static class P2
{

	private readonly struct ForRangeHelper //: IDisposable
	{
		public readonly MemoryOwner<(int startInclusive, int length)> owner;
		public readonly (int startInclusive, int length)[] partitions;
		public readonly Action<(int start, int length)> userAction;

		public ForRangeHelper(MemoryOwner<(int startInclusive, int length)> owner, Action<(int start, int length)> userAction)
		{
			this.owner = owner;
			this.partitions = owner.DangerousGetArray().Array;
			this.userAction = userAction;
		}
		public void Action(int index)
		{
			Unsafe.AsRef(in userAction).Invoke(partitions[index]);
		}

		//public void Dispose()
		//{
		//	owner.Dispose();
		//}
	}

	//public static void ForRange(int startInclusive, int length, int maxParallel = -1, int maxBatchSize = int.MaxValue, int minBatchSize = 0, Action<(int startIndex, int length)> action)



	private static MemoryOwner<(int startInclusive, int length)> _RangeHelper(int start, int length, float batchSizeMultipler)
	{
		__ERROR.Throw(batchSizeMultipler > 0, $"{nameof(batchSizeMultipler)} should be greater than zero");
		var endExclusive = start + length;

		var didCount = 0;
		//number of batches we want
		var batchCount = Math.Min(length, Environment.ProcessorCount);

		//figure out batch size
		var batchSize = length / batchCount;

		batchSize = (int)(batchSize * batchSizeMultipler);
		//batchSize = Math.Max(batchSize, minBatchSize);
		//batchSize = Math.Min(batchSize, maxBatchSize);
		batchSize = Math.Min(batchSize, length);

		//update batchCount bsed on actual batchSize
		if (length % batchSize == 0)
		{
			batchCount = length / batchSize;
		}
		else
		{
			batchCount = (length / batchSize) + 1;
		}


		var owner = MemoryOwner<(int startInclusive, int length)>.Allocate(batchCount);
		var span = owner.Span;

		//calculate batches and put into span
		{
			var batchStartInclusive = start;
			var batchEndExclusive = batchStartInclusive + batchSize;
			var loopIndex = 0;
			while (batchEndExclusive <= endExclusive)
			{
				var thisBatchLength = batchEndExclusive - batchStartInclusive;
				__ERROR.Throw(thisBatchLength == batchSize);
				//do work:  batchStartInclusive, batchSize
				didCount += batchSize;
				span[loopIndex] = (batchStartInclusive, batchSize);

				//increment
				batchStartInclusive += batchSize;
				batchEndExclusive += batchSize;
				loopIndex++;
			}
			var remainder = endExclusive - batchStartInclusive;
			__ERROR.Throw(remainder < batchSize);
			if (remainder > 0)
			{
				//do last part:   batchStartInclusive, remainder
				didCount += remainder;
				span[loopIndex] = (batchStartInclusive, remainder);
			}
			__ERROR.Throw(didCount == length);
		}

		return owner;

		////do the parallel work
		//var helper = new ForRangeHelper(owner, action);
		//return helper;
	}
	public static void RangeFor(int start, int length, Action<(int start, int length)> action)=>RangeFor(start, length,1f, action);
	public static void RangeFor(int start, int length, float batchSizeMultipler, Action<(int start, int length)> action)
	{
		if (length == 0)
		{
			return;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		var helper = new ForRangeHelper(owner, action);

		Parallel.For(0, helper.owner.Length, helper.Action);
	}
	public static ValueTask RangeForEachAsync(int start, int length, Func<(int start, int length), ValueTask> action) =>RangeForEachAsync(start, length, 1f, action);
	public static async ValueTask RangeForEachAsync(int start, int length, float batchSizeMultipler, Func<(int start, int length), ValueTask> action)
	{
		if (length == 0)
		{
			return;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		await Parallel.ForEachAsync<(int start, int length)>(owner.DangerousGetArray(), (batch, cancelToken) => { return action(batch); });
	}
	public static ValueTask RangeActionBlock(int start, int length, Action<(int start, int length)> action) => RangeActionBlock(start, length, 1f, action);
	public static async ValueTask RangeActionBlock(int start, int length, float batchSizeMultipler, Action<(int start, int length)> action)
	{
		if (length == 0)
		{
			return;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		var block = new ActionBlock<(int start, int length)>(action);

		foreach(var item in owner.DangerousGetArray())
		{
			block.Post(item);
		}
		block.Complete();

		
		
		await block.Completion;
	}

	public static Task RangeActionBlock_Task(int start, int length, Action<(int start, int length)> action) => RangeActionBlock_Task(start, length, 1f, action);
	public static Task RangeActionBlock_Task(int start, int length, float batchSizeMultipler, Action<(int start, int length)> action)
	{
		if (length == 0)
		{
			return Task.CompletedTask;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		var block = new ActionBlock<(int start, int length)>(action);

		foreach (var item in owner.DangerousGetArray())
		{
			block.Post(item);
		}
		block.Complete();


		return block.Completion;
	}


	public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
	{
		var options = new ExecutionDataflowBlockOptions
		{
			MaxDegreeOfParallelism = maxDegreeOfParallelism
		};
		if (scheduler != null)
			options.TaskScheduler = scheduler;
		var block = new ActionBlock<T>(body, options);
		await foreach (var item in source)
			block.Post(item);
		block.Complete();
		await block.Completion;
	}


	public static ValueTask RangeAsync(int start, int length, Func<(int start, int length), ValueTask> action) => RangeAsync(start, length, 1f, action);
	public static async ValueTask RangeAsync(int start, int length, float batchSizeMultipler, Func<(int start, int length), ValueTask> action)
	{
		if (length == 0)
		{
			return;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		var array = owner.DangerousGetArray().Array;



		using var tasksOwner = MemoryOwner<Task>.Allocate(owner.Length);
		var tasks = tasksOwner.DangerousGetArray().Array;


		for (var i = 0; i < owner.Length; i++)
		{
			var index = i;


			var task = Task.Run(() =>
			{
				return action(array[index]);
			});
			tasks[index] = task;

			//if (tasksCurrent.Count >= Environment.ProcessorCount)
			//{
			//	if (tasksOld.Count > 0)
			//	{
			//		await Task.WhenAll(tasksOld);
			//		tasksOld.Clear();
			//		var temp = tasksOld;
			//		tasksOld = tasksCurrent;
			//		tasksCurrent = temp;
			//	}

			//}
		}

		await Task.WhenAll(tasks);




	}
	public static Task RangeAsync_Task(int start, int length, Func<(int start, int length), Task> action) => RangeAsync_Task(start, length, 1f, action);
	public static Task RangeAsync_Task(int start, int length, float batchSizeMultipler, Func<(int start, int length), Task> action)
	{
		if (length == 0)
		{
			return Task.CompletedTask;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		var array = owner.DangerousGetArray().Array;



		using var tasksOwner = MemoryOwner<Task>.Allocate(owner.Length);
		var tasks = tasksOwner.DangerousGetArray().Array;


		for (var i = 0; i < owner.Length; i++)
		{
			var index = i;


			var task = Task.Run(() =>
			{
				return action(array[index]);
			});
			tasks[index] = task;
		}

		return Task.WhenAll(tasks);




	}
}
