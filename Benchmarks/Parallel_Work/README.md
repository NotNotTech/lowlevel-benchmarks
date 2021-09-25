# parallel-work-gc-benchmarks
benchmarking ways of parallel processing big spans/data while being considerate of GC



## The Benchmarks

Benchmark code follows this pattern, taken from the `Serial_Default` baseline:
```cs
	/// <summary>
	/// all serial benchmarks try to change only one thing from this default baseline.  
	/// </summary>
	[Benchmark(Baseline = true)]
	public long Serial_Default()
	{
		var keys = dumbWork.keys;  //input data:  random keys in an unordered array

		//the benchmark will generate data, do two writes, and store it here
		Span<Data> output = stackalloc Data[DumbWork.DATA_LENGTH];  //DATA_LENGTH=10000

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
```


## Benchmark output
Below are my findings on my dev PC.  

```cmd
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1237 (20H2/October2020Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


| Method                                 |        Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 |   Allocated |
| -------------------------------------- | ----------: | ---------: | ---------: | ----: | ------: | -------: | -------: | -------: | ----------: |
| Serial_Default                         | 7,124.88 us | 119.812 us | 179.329 us | 1.000 |    0.00 |        - |        - |        - |         5 B |
| Serial_AllocOneObj                     | 6,905.45 us |  64.936 us |  57.564 us | 0.966 |    0.03 |        - |        - |        - |        28 B |
| Serial_AllocOneObjPerLoop              | 7,085.89 us |  86.616 us |  72.329 us | 0.989 |    0.03 |  23.4375 |        - |        - |   240,028 B |
| Serial_NaiveAlloc                      | 7,212.84 us | 128.144 us | 113.597 us | 1.009 |    0.03 | 242.1875 | 242.1875 | 242.1875 |   800,111 B |
| Serial_ByRef                           | 7,028.02 us |  81.712 us |  76.433 us | 0.984 |    0.03 |        - |        - |        - |         4 B |
| Serial_SpanOwner                       | 6,992.25 us |  89.641 us |  74.854 us | 0.976 |    0.03 |        - |        - |        - |         6 B |
| Serial_MemoryOwner                     | 6,843.82 us |  68.362 us |  60.602 us | 0.957 |    0.03 |        - |        - |        - |        47 B |
| Serial_SpanOwner_Array                 | 7,080.25 us | 127.759 us | 113.255 us | 0.991 |    0.04 |        - |        - |        - |         6 B |
| Serial_MemoryOwner_Array               | 6,876.55 us |  75.631 us |  70.746 us | 0.963 |    0.03 |        - |        - |        - |        46 B |
| Serial_SpanOwner_NoUsing               | 7,176.33 us |  84.524 us |  70.581 us | 1.002 |    0.04 | 328.1250 | 328.1250 | 328.1250 | 1,310,861 B |
| Serial_MemoryOwner_NoUsing             | 6,926.58 us |  82.976 us |  69.288 us | 0.967 |    0.03 |        - |        - |        - |        47 B |
| ParallelFor_NoWork                     |    48.51 us |   0.769 us |   0.720 us | 0.007 |    0.00 |   1.0986 |        - |        - |     9,294 B |
| ParallelFor_Lambda                     | 1,026.18 us |  11.536 us |   9.633 us | 0.143 |    0.00 |        - |        - |        - |     9,378 B |
| ParallelFor_Lambda_Fixed               |   951.16 us |  13.355 us |  11.152 us | 0.133 |    0.00 |   0.9766 |        - |        - |     9,350 B |
| ParallelFor_InstanceMethod             |   946.35 us |   7.722 us |   6.448 us | 0.132 |    0.00 |   0.9766 |        - |        - |     9,092 B |
| ParallelHelperFor_Standard             | 1,045.33 us |  20.077 us |  24.656 us | 0.147 |    0.00 |        - |        - |        - |     9,524 B |
| ParallelHelperFor_SpanExtensionLambda  | 1,060.83 us |  20.778 us |  20.407 us | 0.149 |    0.00 |        - |        - |        - |     9,619 B |
| ParallelHelperFor_ArrayExtensionLambda | 1,064.00 us |  14.353 us |  11.206 us | 0.148 |    0.01 |        - |        - |        - |     9,633 B |
| ParallelFor_ParallelForRangeExtension  | 1,020.10 us |  12.134 us |  11.350 us | 0.143 |    0.00 |        - |        - |        - |    15,550 B |

// * Legends *
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Ratio     : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD   : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Gen 0     : GC Generation 0 collects per 1000 operations
  Gen 1     : GC Generation 1 collects per 1000 operations
  Gen 2     : GC Generation 2 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
```



### Thoughts...
- **Speed is good**. This isn't a benchmark on parallel speed, it's looking at allocations.  
  - That said, generally the parallel computation speed seems favorable.  Be aware that verification code is done serially even in Parallel benchmarks.
- **Parallel allocations are unavoidable**. additional object allocations are annoying (but relatively small).   
  - In the benchmarks, allocating one object per loop (benchmark `Serial_AllocOneObjPerLoop` above) allocates 240kb.  Compared to that, the 9kb emitted by the Parallel tests is basically nothing.
- **`MemoryOwner<T>` instead of `SpanOwner<T>`**.  Both are great, but the tiny extra allocation overhead of `MemoryOwner` is insignificant compared to it's flexibility (use in `async/await`, Parallel/Lambdas).  
  - Also compared to the allocation cost of running a `Parallel.For` MemoryOwner allocations are effectively free.
- **Lambdas/delegate parameter capture doesnt hurt**.  Compare the "Lambda" benchmarks such as `ParallelFor_Lambda` with others.  There doesn't seem to be any difference in allocations or execution speed.







