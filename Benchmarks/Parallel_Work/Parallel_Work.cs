using BenchmarkDotNet.Attributes;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Microsoft.Toolkit.HighPerformance.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lowlevel_benchmark.Benchmarks;

/// <summary>
/// benchmark doing work on spans, input and output.   see readme.md in this folder
/// </summary>
//[ShortRunJob]
[MemoryDiagnoser]
public class Parallel_Work
{
	private DumbWork dumbWork = DumbWork.INSTANCE;


	/// <summary>
	/// all serial benchmarks try to change only one thing from this default baseline.  
	/// permutations off of another test should have an additiona `_suffix` attached to that case
	/// </summary>
	[Benchmark(Baseline = true)]
	public long Serial_Default()
	{
		var keys = dumbWork.keys;  //input data:  random keys in an unordered array

		//the benchmark will generate data, do two writes, and store it here
		Span<Data> output = stackalloc Data[DumbWork.DATA_LENGTH]; //DATA_LENGTH=10000

		//make a data, do a write to it, and store it
		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		//loop again writing once more
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}
		//verify benchmark output is correct
		return dumbWork.Verify(output);
	}
	[Benchmark]
	public long Serial_AllocOneObj()
	{
		var keys = dumbWork.keys;
		Span<Data> output = stackalloc Data[DumbWork.DATA_LENGTH];
		var obj = new object();
		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}
		if (obj.GetHashCode() == -1)
		{
			throw new Exception("should not get here");
		}
		return dumbWork.Verify(output);
	}
	[Benchmark]
	public long Serial_AllocOneObjPerLoop()
	{
		var keys = dumbWork.keys;
		Span<Data> output = stackalloc Data[DumbWork.DATA_LENGTH];
		var obj = new object();
		for (var i = 0; i < keys.Length; i++)
		{
			obj = new object();
			if (obj.GetHashCode() == -1)
			{
				throw new Exception("should not get here");
			}

			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}
		if (obj.GetHashCode() == -1)
		{
			throw new Exception("should not get here");
		}
		return dumbWork.Verify(output);
	}
	[Benchmark]
	public long Serial_NaiveAlloc()
	{
		var keys = dumbWork.keys;
		var output = new Data[DumbWork.DATA_LENGTH];

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(output);
	}

	[Benchmark]
	public long Serial_ByRef()
	{
		var keys = dumbWork.keys;
		Span<Data> output = stackalloc Data[DumbWork.DATA_LENGTH];

		for (var i = 0; i < keys.Length; i++)
		{
			ref var data = ref output[i];
			data = new Data(keys[i]);
			data.Write();
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(output);

	}

	[Benchmark]
	public long Serial_SpanOwner()
	{
		var keys = dumbWork.keys;
		using var owner = SpanOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		Span<Data> output = owner.Span;

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(output);
	}
	[Benchmark]
	public long Serial_MemoryOwner()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		Span<Data> output = owner.Span;

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(output);
	}

	[Benchmark]
	public long Serial_SpanOwner_Array()
	{
		var keys = dumbWork.keys;
		using var owner = SpanOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array; //note: backing array may be longer!

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < keys.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(owner.Span);
	}
	[Benchmark]
	public long Serial_MemoryOwner_Array()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array; //note: backing array may be longer!

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < keys.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(owner.Span);
	}
	[Benchmark]
	public long Serial_SpanOwner_NoUsing()
	{
		var keys = dumbWork.keys;
		var owner = SpanOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		Span<Data> output = owner.Span;

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(output);
	}
	[Benchmark]
	public long Serial_MemoryOwner_NoUsing()
	{
		var keys = dumbWork.keys;
		var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		Span<Data> output = owner.Span;

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}

		return dumbWork.Verify(output);
	}

	[Benchmark]
	public unsafe long ParallelFor_NoWork()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		Span<Data> output = owner.Span;

		fixed (Data* p = output)
		{
			var pOutput = p;

			Parallel.For(0, output.Length, (i) =>
			{
				//var key = keys[i];
				//var data = new Data(key);
				//data.Write();
				//pOutput[i] = data;
			});
		}
		fixed (Data* p = output)
		{
			var pOutput = p;

			Parallel.For(0, output.Length, (i) =>
			{
				//pOutput[i].Write();
			});
		}

		return output.Length;// dumbWork.Verify(output);
	}
	[Benchmark]
	public unsafe long ParallelFor_Lambda()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array;   //note: backing array may be longer!

		Parallel.For(0, owner.Length, (i) =>
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		});

		Parallel.For(0, owner.Length, (i) =>
		{
			output[i].Write();
		});


		return dumbWork.Verify(owner.Span);
	}
	[Benchmark]
	public unsafe long ParallelFor_Lambda_Fixed()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		Span<Data> output = owner.Span;

		fixed (Data* p = output)
		{
			var pOutput = p;

			Parallel.For(0, output.Length, (i) =>
			{
				var key = keys[i];
				var data = new Data(key);
				data.Write();
				pOutput[i] = data;
			});
		}
		fixed (Data* p = output)
		{
			var pOutput = p;

			Parallel.For(0, output.Length, (i) =>
			{
				pOutput[i].Write();
			});
		}

		return dumbWork.Verify(output);
	}


	private readonly struct _ParallelFor_InstanceMethod_StructHelper
	{
		public readonly MemoryOwner<Data> owner;
		public readonly Data[] output;
		public readonly long[] keys;

		public _ParallelFor_InstanceMethod_StructHelper(MemoryOwner<Data> owner, long[] keys)
		{
			this.owner = owner;
			this.output = owner.DangerousGetArray().Array;
			this.keys = keys;
		}

		public void _FirstWrite(int i)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}

		public void _SecondWrite(int i)
		{
			output[i].Write();
		}
	}


	[Benchmark]
	public unsafe long ParallelFor_InstanceMethod()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array;   //note: backing array may be longer!

		var delegateHelper = new _ParallelFor_InstanceMethod_StructHelper(owner, keys);


		Parallel.For(0, owner.Length, delegateHelper._FirstWrite);

		Parallel.For(0, owner.Length, delegateHelper._SecondWrite);


		return dumbWork.Verify(owner.Span);
	}


	private readonly struct ParallelHelperFor_Standard_StructHelper_First : IAction
	{
		public readonly MemoryOwner<Data> owner;
		public readonly Data[] output;
		public readonly long[] keys;

		public ParallelHelperFor_Standard_StructHelper_First(MemoryOwner<Data> owner, long[] keys)
		{
			this.owner = owner;
			this.output = owner.DangerousGetArray().Array;
			this.keys = keys;
		}

		public void Invoke(int i)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}

	}

	private readonly struct ParallelHelperFor_Standard_StructHelper_Second : IAction
	{
		public readonly MemoryOwner<Data> owner;
		public readonly Data[] output;
		public readonly long[] keys;

		public ParallelHelperFor_Standard_StructHelper_Second(MemoryOwner<Data> owner, long[] keys)
		{
			this.owner = owner;
			this.output = owner.DangerousGetArray().Array;
			this.keys = keys;
		}


		public void Invoke(int i)
		{
			output[i].Write();
		}
	}

	[Benchmark]
	public unsafe long ParallelHelperFor_Standard()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array;   //note: backing array may be longer!


		var delegateHelperFirst = new ParallelHelperFor_Standard_StructHelper_First(owner, keys);
		var delegateHelperSecond = new ParallelHelperFor_Standard_StructHelper_Second(owner, keys);


		ParallelHelper.For(0, owner.Length, in delegateHelperFirst);
		ParallelHelper.For(0, owner.Length, in delegateHelperSecond);

		return dumbWork.Verify(owner.Span);
	}
	[Benchmark]
	public unsafe long ParallelHelperFor_SpanExtensionLambda()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.Span;


		output._ParallelForEach((output, i) =>
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		});

		output._ParallelForEach((output, i) =>
		{
			output[i].Write();
		});


		return dumbWork.Verify(owner.Span);
	}
	[Benchmark]
	public unsafe long ParallelHelperFor_ArrayExtensionLambda()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array;  //note: backing array may be longer!


		output._ParallelForEach(0, owner.Length, (output, i) =>
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		});

		output._ParallelForEach(0, owner.Length, (output, i) =>
		{
			output[i].Write();
		});


		return dumbWork.Verify(owner.Span);
	}
	/// <summary>
	/// add ForRange sample, from https://github.com/dotnet/samples/tree/2cf486af936261b04a438ea44779cdc26c613f98/csharp/parallel/ParallelExtensionsExtras
	/// </summary>
	/// <returns></returns>
	[Benchmark]
	public unsafe long ParallelFor_ParallelForRangeExtension()
	{
		var keys = dumbWork.keys;
		using var owner = MemoryOwner<Data>.Allocate(DumbWork.DATA_LENGTH);
		var output = owner.DangerousGetArray().Array;  //note: backing array may be longer!



		Extras.ParallelAlgorithms.ForRange(0, owner.Length, (startInclusive, endExclusive) => {
		
			for(var i= startInclusive; i < endExclusive; i++)
			{
				var key = keys[i];
				var data = new Data(key);
				data.Write();
				output[i] = data;
			}
		
		});

		Extras.ParallelAlgorithms.ForRange(0, owner.Length, (startInclusive, endExclusive) => {

			for (var i = startInclusive; i < endExclusive; i++)
			{
				output[i].Write();
			}

		});

		return dumbWork.Verify(owner.Span);
	}

}
