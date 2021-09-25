using lowlevel_benchmark.Helpers;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace lowlevel_benchmark.Benchmarks;

/// <summary>
/// Various ParallelHelper style static methods to experiment with efficient use of `Parallel.For()` style workflows
/// </summary>
public static class P2
{


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
		batchSize = Math.Min(batchSize, length);
		batchSize = Math.Max(1, batchSize);

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

	}

	private static SpanOwner<(int startInclusive, int length)> _RangeHelper_Span(int start, int length, float batchSizeMultipler)
	{
		__ERROR.Throw(batchSizeMultipler > 0, $"{nameof(batchSizeMultipler)} should be greater than zero");
		var endExclusive = start + length;

		var didCount = 0;
		//number of batches we want
		var batchCount = Math.Min(length, Environment.ProcessorCount);

		//figure out batch size
		var batchSize = length / batchCount;

		batchSize = (int)(batchSize * batchSizeMultipler);
		batchSize = Math.Min(batchSize, length);
		batchSize = Math.Max(1, batchSize);

		//update batchCount bsed on actual batchSize
		if (length % batchSize == 0)
		{
			batchCount = length / batchSize;
		}
		else
		{
			batchCount = (length / batchSize) + 1;
		}


		var owner = SpanOwner<(int startInclusive, int length)>.Allocate(batchCount);
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
	private readonly struct ForRangeHelper //: IDisposable
	{
		//public readonly MemoryOwner<(int startInclusive, int length)> owner;
		public readonly (int startInclusive, int length)[] partitions;
		public readonly Action<(int start, int length)> userAction;
		public readonly int partitionsCount;

		public ForRangeHelper((int startInclusive, int length)[] partitions,int partitionsCount, Action<(int start, int length)> userAction)
		{
			//this.owner = owner;
			//this.partitions = owner.DangerousGetArray().Array;
			this.partitions= partitions;
			this.partitionsCount= partitionsCount;
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
	public static void RangeFor(int start, int length, Action<(int start, int length)> action) => RangeFor(start, length, 1f, action);
	public static void RangeFor(int start, int length, float batchSizeMultipler, Action<(int start, int length)> action)
	{
		if (length == 0)
		{
			return;
		}
		using var owner = _RangeHelper(start, length, batchSizeMultipler);

		var helper = new ForRangeHelper(owner.DangerousGetArray().Array,owner.Length, action);

		Parallel.For(0, owner.Length, helper.Action);
	}
	public static void RangeFor_Span(int start, int length, Action<(int start, int length)> action) => RangeFor(start, length, 1f, action);
	public static void RangeFor_Span(int start, int length, float batchSizeMultipler, Action<(int start, int length)> action)
	{
		if (length == 0)
		{
			return;
		}
		using var owner = _RangeHelper_Span(start, length, batchSizeMultipler);

		var helper = new ForRangeHelper(owner.DangerousGetArray().Array, owner.Length, action);

		Parallel.For(0, owner.Length, helper.Action);
	}
	public static ValueTask RangeForEachAsync(int start, int length, Func<(int start, int length), ValueTask> action) => RangeForEachAsync(start, length, 1f, action);
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

		foreach (var item in owner.DangerousGetArray())
		{
			block.Post(item);
		}
		block.Complete();



		await block.Completion;
	}
	public static ValueTask RangeForEachAsync_Span(int start, int length, Func<(int start, int length), ValueTask> action) => RangeForEachAsync_Span(start, length, 1f, action);
	public static ValueTask RangeForEachAsync_Span(int start, int length, float batchSizeMultipler, Func<(int start, int length), ValueTask> action)
	{
		if (length == 0)
		{
			return ValueTask.CompletedTask;
		}
		using var owner = _RangeHelper_Span(start, length, batchSizeMultipler);

		return RangeForEachAsync_Span_Helper(owner.DangerousGetArray(), action);
	}
	private static async ValueTask RangeForEachAsync_Span_Helper(ArraySegment<(int start, int length)> spanOwnerDangerousArray, Func<(int start, int length), ValueTask> action)
	{
		await Parallel.ForEachAsync<(int start, int length)>(spanOwnerDangerousArray, (batch, cancelToken) => { return action(batch); });
	}


	public static Task RangeForEachAsync_Span_TASK(int start, int length, Func<(int start, int length), ValueTask> action) => RangeForEachAsync_Span_TASK(start, length, 1f, action);
	public static Task RangeForEachAsync_Span_TASK(int start, int length, float batchSizeMultipler, Func<(int start, int length), ValueTask> action)
	{
		if (length == 0)
		{
			return Task.CompletedTask;
		}
		using var owner = _RangeHelper_Span(start, length, batchSizeMultipler);

		return RangeForEachAsync_Span_TASK_Helper(owner.DangerousGetArray(), action);
	}
	private static async Task RangeForEachAsync_Span_TASK_Helper(ArraySegment<(int start, int length)> spanOwnerDangerousArray, Func<(int start, int length), ValueTask> action)
	{
		await Parallel.ForEachAsync<(int start, int length)>(spanOwnerDangerousArray, (batch, cancelToken) => { return action(batch); });
	}


	private readonly struct RangeForEachAsyncHelper //: IDisposable
	{
		//public readonly MemoryOwner<(int startInclusive, int length)> owner;
		//public readonly (int startInclusive, int length)[] partitions;
		//public readonly int partitionsCount;
		public readonly Func<(int start, int length), ValueTask>  userAction;

		public RangeForEachAsyncHelper(Func<(int start, int length), ValueTask> userAction)
		{
			//this.owner = owner;
			//this.partitions = owner.DangerousGetArray().Array;
			//this.partitions = partitions;
			//this.partitionsCount = partitionsCount;
			this.userAction = userAction;
		}
		public ValueTask Action((int start, int length) batch, CancellationToken token)
		{
			return Unsafe.AsRef(in userAction).Invoke(batch);
		}

		//public void Dispose()
		//{
		//	owner.Dispose();
		//}
	}
	public static ValueTask RangeForEachAsync_Span_Helper(int start, int length, Func<(int start, int length), ValueTask> action) => RangeForEachAsync_Span_Helper(start, length, 1f, action);
	public static ValueTask RangeForEachAsync_Span_Helper(int start, int length, float batchSizeMultipler, Func<(int start, int length), ValueTask> action)
	{
		if (length == 0)
		{
			return ValueTask.CompletedTask;
		}
		using var owner = _RangeHelper_Span(start, length, batchSizeMultipler);

		return RangeForEachAsync_Span_Helper_Helper(owner.DangerousGetArray(), action);
	}
	private static async ValueTask RangeForEachAsync_Span_Helper_Helper(ArraySegment<(int start, int length)> spanOwnerDangerousArray, Func<(int start, int length), ValueTask> action)
	{


		//await Parallel.ForEachAsync<(int start, int length)>(spanOwnerDangerousArray, (batch, cancelToken) => {
		//	return Unsafe.AsRef(in action).Invoke(batch);
		//});
		var helper = new RangeForEachAsyncHelper(action);
		
		await Parallel.ForEachAsync<(int start, int length)>(spanOwnerDangerousArray, helper.Action);
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

